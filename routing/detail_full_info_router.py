from fastapi import APIRouter, HTTPException, Depends
from typing import List, Optional
from pydantic import BaseModel

from S3.s3_service import S3Service
from dependencies import (
    get_article_attributes_service,
    get_et_producer_service,
    get_article_images_service,
    get_articles_service,
    get_suppliers_service,
    get_s3_service,
)
from schemas.article_attributes_schema import ArticleAttributesSchema
from schemas.aticle_image_schema import ArticleImageSchema
from schemas.et_producer_schema import EtProducerSchema
from schemas.full_detail_info_schema import FullDetailInfoSchema
from schemas.suppliers_schema import SuppliersSchema
from services.article_attributes_service import ArticleAttributesService
from services.article_images_service import ArticleImagesService
from services.articles_service import ArticlesService
from services.et_producer_service import EtProducerService
from services.suppliers_service import SuppliersService

router = APIRouter(
    prefix="/detail-full-info",
    tags=["Detail full info"],
)

# Основной эндпоинт
@router.get("/{supplier}/{article}", response_model=FullDetailInfoSchema)
async def get_full_detail_info(
        supplier: str,
        article: str,
        article_attributes_service: ArticleAttributesService = Depends(get_article_attributes_service),
        et_producer_service: EtProducerService = Depends(get_et_producer_service),
        article_images_service: ArticleImagesService = Depends(get_article_images_service),
        articles_service: ArticlesService = Depends(get_articles_service),
        suppliers_service: SuppliersService = Depends(get_suppliers_service),
        s3_service: S3Service = Depends(get_s3_service)
):
    """
    Эндпоинт для получения полной информации об артикуле:
    - Атрибуты артикула
    - URL-адреса изображений
    - Информация о поставщике
    """
    try:
        # Шаг 1: Нормализация артикула
        normalized_article = await get_normalized_article(article, articles_service)

        # Шаг 2: Получение ID поставщика
        supplier_from_jc = await et_producer_service.get_producers_by_name(supplier)
        supplier_id = await get_supplier_id(supplier, et_producer_service, suppliers_service)

        # Шаг 3: Получение атрибутов и изображений
        detail_attribute = await article_attributes_service.get_attribute_by_filter(supplier_id, normalized_article)
        images = await article_images_service.get_images_by_filter(supplier_id, normalized_article)

        # Шаг 4: Генерация URL для изображений
        img_urls = await fetch_image_urls(images, s3_service)

        # Шаг 5: Получение данных поставщика из TD
        supplier_from_td = await get_supplier_from_td(supplier, supplier_from_jc, suppliers_service)

        # Возврат ответа
        return FullDetailInfoSchema(
            normalized_article=normalized_article,
            detail_attribute=detail_attribute,
            img_urls=img_urls,
            supplier_from_jc=supplier_from_jc,
            supplier_from_td=supplier_from_td  # Устанавливаем результат
        )
    except HTTPException as e:
        raise e
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Внутренняя ошибка сервера: {str(e)}")

# Вспомогательная функция: Нормализация артикула
async def get_normalized_article(article: str, articles_service: ArticlesService) -> str:
    """
    Проверяет наличие артикула в базе данных и возвращает нормализованный артикул.
    Если артикул не найден, возвращает исходный.
    """
    data = await articles_service.get_articles_by_found_string(article)
    return data.DataSupplierArticleNumber if data else article


# Вспомогательная функция: Получение ID поставщика
async def get_supplier_id(
        supplier: str,
        et_producer_service: EtProducerService,
        suppliers_service: SuppliersService
) -> int:
    """
    Получает ID поставщика на основе различных условий:
    - Если TecDoc ID равен 0, ищет поставщика в альтернативных источниках.
    - В противном случае возвращает TecDoc ID.
    """
    supplier_from_jc: EtProducerSchema = await et_producer_service.get_producers_by_name(supplier)

    # Если текущий ID не совпадает с realid, получаем обновлённого поставщика
    if int(supplier_from_jc.id) != int(supplier_from_jc.realid):
        supplier_from_jc = await et_producer_service.get_producer_by_id(int(supplier_from_jc.realid))

    # Если TecDoc ID равен 0, пытаемся найти поставщика через SuppliersService
    if int(supplier_from_jc.tecdocSupplierId) == 0:
        supplier_from_td: Optional[SuppliersSchema] = await suppliers_service.get_suppliers_by_description_case_ignore(
            supplier)

        # Если не найдено, ищем через префиксы
        if supplier_from_jc.prefix and not supplier_from_td:
            supplier_from_td = await suppliers_service.get_suppliers_by_matchcode(supplier_from_jc.prefix)
            if not supplier_from_td:
                supplier_from_td = await suppliers_service.get_suppliers_by_matchcode(supplier_from_jc.marketPrefix)

        # Если поставщик не найден, возвращаем ошибку
        if not supplier_from_td:
            raise HTTPException(status_code=404, detail="Поставщик не найден")

        return int(supplier_from_td.id)
    else:
        return supplier_from_jc.tecdocSupplierId

async def get_supplier_from_td(
    supplier: str,
    supplier_from_jc: EtProducerSchema,
    suppliers_service: SuppliersService
) -> Optional[SuppliersSchema]:
    """
    Ищет поставщика в TD по описанию, префиксу или marketPrefix.
    """
    # Попытка найти поставщика по описанию
    supplier_from_td: Optional[SuppliersSchema] = await suppliers_service.get_suppliers_by_description_case_ignore(supplier)

    # Если не найдено, ищем через префиксы
    if supplier_from_jc.prefix and not supplier_from_td:
        supplier_from_td = await suppliers_service.get_suppliers_by_matchcode(supplier_from_jc.prefix)
        if not supplier_from_td:
            supplier_from_td = await suppliers_service.get_suppliers_by_matchcode(supplier_from_jc.marketPrefix)

    return supplier_from_td

# Вспомогательная функция: Получение URL изображений
async def fetch_image_urls(images: List[ArticleImageSchema], s3_service: S3Service) -> List[str]:
    """
    Генерирует URL для всех доступных изображений.
    Если URL не может быть получен, изображение пропускается.
    """
    return [
        s3_service.get_image_url(image.PictureName)
        for image in images if s3_service.get_image_url(image.PictureName) is not None
    ]

