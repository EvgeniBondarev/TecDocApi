from typing import List

from fastapi import APIRouter, Depends

from dependencies import get_article_cross_service, get_article_oe_service, get_suppliers_service, get_articles_service
from schemas.article_cross_schema import ArticleCrossSchema
from schemas.article_oe_schema import ArticleOESchema
from services.article_cross_service import ArticleCrossService
from services.article_oe_service import ArticleOEService
from services.articles_service import ArticlesService
from services.suppliers_service import SuppliersService
from services.utils.search_preparation import get_normalized_article

router = APIRouter(
    prefix="/tec-doc-cross",
    tags=["TecDocCross"]
)

@router.get("/cross/{article}", response_model=List[ArticleOESchema])
async def get_by_article(
    article: str,
    article_cross_service: ArticleCrossService = Depends(get_article_cross_service),
    article_oe_service: ArticleOEService = Depends(get_article_oe_service),
    suppliers_service: SuppliersService = Depends(get_suppliers_service),
    articles_service: ArticlesService = Depends(get_articles_service)
):
    """Получить записи по article."""

    # Получаем уникальные OENs по article
    OENs = await article_cross_service.get_articles_by_article(article)
    if not OENs:
        normalized_article = await get_normalized_article(article, articles_service)
        OENs = await article_cross_service.get_articles_by_article(normalized_article)
        if not OENs:
            return []  # Если OENs пуст, возвращаем пустой список

    unique_OENs = list({(item.OENbr, item.SupplierId): item for item in OENs}.values())

    # Собираем все supplier_id для запроса к suppliers_service
    supplier_ids = {oen.SupplierId for oen in unique_OENs if oen.SupplierId is not None}
    if not supplier_ids:
        return []  # Если supplier_ids пуст, возвращаем пустой список

    # Получаем всех поставщиков за один запрос
    suppliers = await suppliers_service.get_suppliers_by_ids(supplier_ids)
    if not suppliers:
        return []  # Если suppliers пуст, возвращаем пустой список

    supplier_map = {supplier.id: supplier for supplier in suppliers}

    # Собираем все OENbr и SupplierId для запроса к article_oe_service
    oen_supplier_pairs = [(oen.OENbr, oen.SupplierId) for oen in unique_OENs if oen.OENbr is not None and oen.SupplierId is not None]
    if not oen_supplier_pairs:
        return []  # Если oen_supplier_pairs пуст, возвращаем пустой список

    # Получаем все crosses за один запрос
    crosses = await article_oe_service.get_articles_by_oen_supplier_pairs(oen_supplier_pairs)
    if not crosses:
        return []  # Если crosses пуст, возвращаем пустой список

    unique_crosses = list({(item.datasupplierarticlenumber, item.supplierid): item for item in crosses}.values())

    # Добавляем информацию о поставщике в каждый cross
    for cross in unique_crosses:
        if cross.supplierid in supplier_map:
            cross.supplier = supplier_map[cross.supplierid]
        else:
            cross.supplier = None  # Если поставщик не найден, устанавливаем None

    return unique_crosses
