from fastapi import APIRouter, Depends
from typing import List

from dependencies import get_articles_service
from schemas.articles_schema import ArticlesSchema
from schemas.response_schema import ResponseSchema

router = APIRouter(
    prefix="/articles",
    tags=["Articles"]
)


@router.get("/{supplier_id}/{article_number}", response_model=ArticlesSchema)
async def get_article_by_id(supplier_id: int, article_number: str, service=Depends(get_articles_service)):
    """Получить статью по идентификатору (supplierId + DataSupplierArticleNumber)."""
    return await service.get_article_by_id(supplier_id, article_number)



@router.get("/supplier/{supplier_id}", response_model=List[ArticlesSchema])
async def get_articles_by_supplier(supplier_id: int, service=Depends(get_articles_service)):
    """Получить статьи по supplierId."""
    return service.get_articles_by_supplier(supplier_id)


@router.get("/search/{found_string}", response_model=List[ArticlesSchema])
async def get_articles_by_found_string(found_string: str, service=Depends(get_articles_service)):
    """Получить статьи по полю FoundString."""
    return  await service.get_articles_by_found_string(found_string)

