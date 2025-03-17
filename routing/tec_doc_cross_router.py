from typing import List, Set, Dict, Tuple
from fastapi import APIRouter, Depends
from dependencies import (
    get_article_cross_service, get_article_oe_service, get_suppliers_service,
    get_articles_service, get_manufacturer_service
)
from schemas.article_oe_schema import ArticleOESchema
from schemas.manufacturer_schema import ManufacturerSchema
from schemas.suppliers_schema import SuppliersSchema
from services.article_cross_service import ArticleCrossService
from services.article_oe_service import ArticleOEService
from services.articles_service import ArticlesService
from services.manufacturer_service import ManufacturerService
from services.suppliers_service import SuppliersService
from services.utils.search_preparation import get_normalized_article

router = APIRouter(
    prefix="/tec-doc-cross",
    tags=["TecDocCross"]
)

async def get_unique_oens(
    article: str,
    article_cross_service: ArticleCrossService,
    articles_service: ArticlesService,
) -> List[ArticleOESchema]:
    """Получить уникальные OENs по артикулу."""
    OENs = await article_cross_service.get_articles_by_article(article)
    if not OENs:
        normalized_article = await get_normalized_article(article, articles_service)
        OENs = await article_cross_service.get_articles_by_article(normalized_article)
        if not OENs:
            return []
    return list({(item.OENbr, item.SupplierId): item for item in OENs}.values())

async def get_suppliers_map(
    supplier_ids: Set[int],
    suppliers_service: SuppliersService,
) -> Dict[int, SuppliersSchema]:
    """Получить словарь поставщиков по их ID."""
    suppliers = await suppliers_service.get_suppliers_by_ids(supplier_ids)
    return {supplier.id: supplier for supplier in suppliers} if suppliers else {}

async def get_manufacturers_map(
    manufacturer_ids: Set[int],
    manufacturer_service: ManufacturerService,
) -> Dict[int, ManufacturerSchema]:
    """Получить словарь производителей по их ID."""
    manufacturers = await manufacturer_service.get_manufacturers_by_ids(manufacturer_ids)
    return {manufacturer.id: manufacturer for manufacturer in manufacturers} if manufacturers else {}

async def get_unique_crosses(
    oen_supplier_pairs: List[Tuple[str, int]],
    article_oe_service: ArticleOEService,
) -> List[ArticleOESchema]:
    """Получить уникальные кроссы по парам OENbr и SupplierId."""
    crosses = await article_oe_service.get_articles_by_oen_supplier_pairs(oen_supplier_pairs)
    return list({(item.datasupplierarticlenumber, item.supplierid): item for item in crosses}.values()) if crosses else []

async def enrich_crosses_with_suppliers_and_manufacturers(
    crosses: List[ArticleOESchema],
    supplier_map: Dict[int, SuppliersSchema],
    manufacturer_map: Dict[int, ManufacturerSchema],
) -> List[ArticleOESchema]:
    """Добавить информацию о поставщиках и производителях в кроссы."""
    for cross in crosses:
        cross.supplier = supplier_map.get(cross.supplierid)
        cross.manufacturer = manufacturer_map.get(cross.manufacturerId)
    return crosses

@router.get("/cross/{article}", response_model=List[ArticleOESchema])
async def get_by_article(
    article: str,
    article_cross_service: ArticleCrossService = Depends(get_article_cross_service),
    article_oe_service: ArticleOEService = Depends(get_article_oe_service),
    suppliers_service: SuppliersService = Depends(get_suppliers_service),
    articles_service: ArticlesService = Depends(get_articles_service),
    manufacturer_service: ManufacturerService = Depends(get_manufacturer_service),
):
    """Получить записи по article."""
    # Получаем уникальные OENs
    unique_OENs = await get_unique_oens(article, article_cross_service, articles_service)
    if not unique_OENs:
        return []

    # Получаем поставщиков
    supplier_ids = {oen.SupplierId for oen in unique_OENs if oen.SupplierId is not None}
    supplier_map = await get_suppliers_map(supplier_ids, suppliers_service)

    # Получаем производителей
    manufacturer_ids = {oen.manufacturerId for oen in unique_OENs if oen.manufacturerId is not None}
    manufacturer_map = await get_manufacturers_map(manufacturer_ids, manufacturer_service)

    # Получаем кроссы
    oen_supplier_pairs = [(oen.OENbr, oen.SupplierId) for oen in unique_OENs if oen.OENbr is not None and oen.SupplierId is not None]
    unique_crosses = await get_unique_crosses(oen_supplier_pairs, article_oe_service)

    # Обогащаем кроссы информацией о поставщиках и производителях
    enriched_crosses = await enrich_crosses_with_suppliers_and_manufacturers(
        unique_crosses, supplier_map, manufacturer_map
    )

    return enriched_crosses