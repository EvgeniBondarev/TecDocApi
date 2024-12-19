from fastapi import APIRouter, HTTPException, Depends

from S3.s3_service import S3Service
from dependencies import (
    get_article_attributes_service,
    get_et_producer_service,
    get_article_images_service,
    get_articles_service,
    get_suppliers_service,
    get_s3_service,
)

from schemas.full_detail_info_schema import FullDetailInfoSchema
from services.article_attributes_service import ArticleAttributesService
from services.article_images_service import ArticleImagesService
from services.articles_service import ArticlesService
from services.et_producer_service import EtProducerService
from services.suppliers_service import SuppliersService
from services.utils.search_preparation import get_supplier_id, get_normalized_article, fetch_image_urls, \
    fetch_image_by_supplier_code, get_supplier_from_td

router = APIRouter(
    prefix="/detail-full-info",
    tags=["Detail full info"],
)

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
    article = article.replace(" ", "")
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

        if supplier_from_jc.marketPrefix is not None:
            img_by_supplier = await fetch_image_by_supplier_code(supplier_from_jc.marketPrefix, article, s3_service)
            if img_by_supplier is not None:
                img_urls.append(img_by_supplier)

        # Шаг 5: Получение данных поставщика из TD
        supplier_from_td = await get_supplier_from_td(supplier, supplier_from_jc, suppliers_service)

        return FullDetailInfoSchema(
            normalized_article=normalized_article,
            detail_attribute=detail_attribute,
            img_urls=img_urls,
            supplier_from_jc=supplier_from_jc,
            supplier_from_td=supplier_from_td,
        )
    except HTTPException as e:
        raise e
    except Exception as e:
        raise HTTPException(status_code=500, detail=f"Внутренняя ошибка сервера: {str(e)}")




