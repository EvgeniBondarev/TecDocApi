from fastapi import APIRouter, Depends
from typing import List

from dependencies import get_articles_service, get_suppliers_service
from schemas.suppliers_schema import SuppliersSchema
from services.articles_service import ArticlesService
from services.suppliers_service import SuppliersService
from services.utils.search_preparation import get_normalized_article

router = APIRouter(
    prefix="/suppliers",
    tags=["Supplier"]
)


@router.get("/{article}", response_model=List[SuppliersSchema])
async def get_suppliers_by_article(article: str, articles_service : ArticlesService=Depends(get_articles_service),
                                                 suppliers_service: SuppliersService = Depends(get_suppliers_service)):
    normalized_article = await get_normalized_article(article, articles_service)
    articles = await articles_service.get_articles_by_data_supplier_article_number(normalized_article)
    suppliers = [await suppliers_service.get_supplier_by_id(art.supplierId) for art in articles]
    return suppliers


