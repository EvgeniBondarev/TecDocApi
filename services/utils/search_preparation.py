from http.client import HTTPException
from typing import Optional, List

from S3.s3_service import S3Service
from schemas.articles_schema import ArticlesSchema
from schemas.aticle_image_schema import ArticleImageSchema
from schemas.et_producer_schema import EtProducerSchema
from schemas.suppliers_schema import SuppliersSchema
from services.articles_service import ArticlesService
from services.et_producer_service import EtProducerService
from services.suppliers_service import SuppliersService


async def get_normalized_article(article: str, articles_service: ArticlesService) -> str:
    """
    Проверяет наличие артикула в базе данных и возвращает нормализованный артикул.
    Если артикул не найден, возвращает исходный.
    """
    data = await articles_service.get_articles_by_found_string(article)
    return data.DataSupplierArticleNumber if data else article

async def get_normalized_article_schema(article: str, articles_service: ArticlesService) -> ArticlesSchema:
    """
    Проверяет наличие артикула в базе данных и возвращает нормализованный артикул.
    Если артикул не найден, возвращает исходный.
    """
    data = await articles_service.get_articles_by_found_string(article)
    return data if data else None

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

    if supplier_from_jc is None:
        return 0

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
            return 0

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

async def fetch_image_urls(images: List[ArticleImageSchema], s3_service: S3Service) -> List[str]:
    """
    Генерирует URL для всех доступных изображений.
    Если URL не может быть получен, изображение пропускается.
    """
    unique_picture_names = set()
    urls = []

    for image in images:
        picture_name_lower = image.PictureName.replace("JPG", "jpg")
        if picture_name_lower not in unique_picture_names:
            unique_picture_names.add(picture_name_lower)

            url = s3_service.get_image_url(picture_name_lower, f"TD2018/images/{image.PictureName.split('_')[0]}")
            if url is not None:
                urls.append(url)
    return urls

async def fetch_image_by_supplier_code(supplier: str, article: str, s3_service: S3Service) -> str:
    """
    Генерирует URL для всех доступных изображений.
    Если URL не может быть получен, изображение пропускается.
    """
    return s3_service.get_image_url(f"{article}.jpg", f"{supplier}")
