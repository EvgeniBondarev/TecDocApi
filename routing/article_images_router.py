from fastapi import APIRouter, Depends
from typing import List
from dependencies import get_article_images_service
from schemas.aticle_image_schema import ArticleImageSchema
from schemas.response_schema import ResponseSchema

router = APIRouter(
    prefix="/article-images",
    tags=["Article Images"]
)

@router.get("/", response_model=ResponseSchema[List[ArticleImageSchema]])
async def get_all_images(service=Depends(get_article_images_service)):
    """Получить все изображения товаров."""
    try:
        images = await service.get_all_images()
        if not images:
            return ResponseSchema[
                List[ArticleImageSchema]
            ](error=True, message="No images found.", result=[])
        return ResponseSchema[
            List[ArticleImageSchema]
        ](error=False, message="Images retrieved successfully.", result=images)
    except Exception as e:
        return ResponseSchema[
            List[ArticleImageSchema]
        ](error=True, message=f"Error: {str(e)}", result=[])

@router.get("/{supplier_id}/{article_number}", response_model=ResponseSchema[List[ArticleImageSchema]])
async def get_images_by_filter(supplier_id: int, article_number: str, service=Depends(get_article_images_service)):
    """Получить изображения товара по фильтру (поставщик и артикул)."""
    try:
        images = await service.get_images_by_filter(supplier_id, article_number)
        if not images:
            return ResponseSchema[
                List[ArticleImageSchema]
            ](error=True, message="No images found for this article.", result=[])
        return ResponseSchema[
            List[ArticleImageSchema]
        ](error=False, message="Images retrieved successfully.", result=images)
    except Exception as e:
        return ResponseSchema[
            List[ArticleImageSchema]
        ](error=True, message=f"Error: {str(e)}", result=[])

@router.get("/supplier/{supplier_id}", response_model=ResponseSchema[List[ArticleImageSchema]])
async def get_images_by_supplier(supplier_id: int, service=Depends(get_article_images_service)):
    """Получить все изображения для данного поставщика."""
    try:
        images = await service.get_images_by_supplier(supplier_id)
        if not images:
            return ResponseSchema[
                List[ArticleImageSchema]
            ](error=True, message="No images found for this supplier.", result=[])
        return ResponseSchema[
            List[ArticleImageSchema]
        ](error=False, message="Images retrieved successfully.", result=images)
    except Exception as e:
        return ResponseSchema[
            List[ArticleImageSchema]
        ](error=True, message=f"Error: {str(e)}", result=[])
