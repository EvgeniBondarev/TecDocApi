from fastapi import APIRouter, Depends, HTTPException
from typing import List
from dependencies import get_article_attributes_service
from schemas.response_schema import ResponseSchema
from schemas.article_attributes_schema import ArticleAttributesSchema

router = APIRouter(
    prefix="/article-attributes",
    tags=["Article Attributes"]
)

@router.get("/", response_model=ResponseSchema[ArticleAttributesSchema])
async def get_all_attributes(service=Depends(get_article_attributes_service)):
    try:
        attributes = await service.get_all_attributes()
        if not attributes:
            return ResponseSchema[
                ArticleAttributesSchema](
                    error=True,
                    message="No attributes found.",
                    result=[]
                )
        return ResponseSchema[
            ArticleAttributesSchema](
                error=False,
                message="Attributes retrieved successfully.",
                result=attributes
            )
    except Exception as e:
        return ResponseSchema[
            ArticleAttributesSchema](
                error=True,
                message=f"An error occurred: {str(e)}",
                result=[]
            )

@router.get("/{supplier_id}/{article}", response_model=ResponseSchema[ArticleAttributesSchema])
async def get_attribute_by_filter(supplier_id: int, article: str, service=Depends(get_article_attributes_service)):
    try:
        attribute = await service.get_attribute_by_filter(supplier_id, article)
        if not attribute:
            return ResponseSchema[
                ArticleAttributesSchema](
                    error=True,
                    message="Attribute not found.",
                    result=[]
                )
        return ResponseSchema[
            ArticleAttributesSchema](
                error=False,
                message="Attribute retrieved successfully.",
                result=attribute
            )
    except Exception as e:
        return ResponseSchema[
            ArticleAttributesSchema](
                error=True,
                message=f"An error occurred: {str(e)}",
                result=[]
            )

@router.get("/supplier/{supplier_id}", response_model=ResponseSchema[ArticleAttributesSchema])
async def get_attributes_by_supplier(supplier_id: int, service=Depends(get_article_attributes_service)):
    try:
        attributes = await service.get_attributes_by_supplier(supplier_id)
        if not attributes:
            return ResponseSchema[
                ArticleAttributesSchema](
                    error=True,
                    message="No attributes found for this supplier.",
                    result=[]
                )
        return ResponseSchema[
            ArticleAttributesSchema](
                error=False,
                message="Attributes retrieved successfully.",
                result=attributes
            )
    except Exception as e:
        return ResponseSchema[
            ArticleAttributesSchema](
                error=True,
                message=f"An error occurred: {str(e)}",
                result=[]
            )
