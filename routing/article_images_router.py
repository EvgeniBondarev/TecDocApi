from fastapi import APIRouter, Depends
from typing import List
from dependencies import get_article_images_service
from schemas.aticle_image_schema import ArticleImageSchema
from schemas.response_schema import ResponseSchema

router = APIRouter(
    prefix="/article-images",
    tags=["Article Images"]
)


@router.get("/{supplier_id}/{article_number}", response_model=List[ArticleImageSchema])
async def get_images_by_filter(supplier_id: int, article_number: str, service=Depends(get_article_images_service)):
    """Получить изображения товара по фильтру (поставщик и артикул)."""
    return await service.get_images_by_filter(supplier_id, article_number)

@router.get("/supplier/{supplier_id}/", response_model=List[ArticleImageSchema])
async def get_images_by_supplier(supplier_id: int, service=Depends(get_article_images_service)):
    """Получить все изображения для данного поставщика."""
    return await service.get_images_by_supplier(supplier_id)
