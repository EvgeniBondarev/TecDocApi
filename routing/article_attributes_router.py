from fastapi import APIRouter, Depends, HTTPException
from typing import List
from dependencies import get_article_attributes_service
from schemas.response_schema import ResponseSchema
from schemas.article_attributes_schema import ArticleAttributesSchema

router = APIRouter(
    prefix="/article-attributes",
    tags=["Article Attributes"]
)


@router.get("/{supplier_id}/{article}/", response_model=List[ArticleAttributesSchema])
async def get_attribute_by_filter(supplier_id: int, article: str, service=Depends(get_article_attributes_service)):
    return await service.get_attribute_by_filter(supplier_id, article)


@router.get("/supplier/{supplier_id}/", response_model=List[ArticleAttributesSchema])
async def get_attributes_by_supplier(supplier_id: int, service=Depends(get_article_attributes_service)):
     return await service.get_attributes_by_supplier(supplier_id)
