from fastapi import APIRouter, Depends, Query
from typing import List, Optional
from dependencies import get_product_service
from schemas.pr_product_response_schema import PrProductResponseSchema

router = APIRouter(
    prefix="/pr-part",
    tags=["PrParts"]
)

from fastapi import HTTPException


@router.get("/", response_model=List[PrProductResponseSchema])
async def get_filtered_parts(
        article: Optional[str] = Query(None),
        brand: Optional[str] = Query(None),
        service=Depends(get_product_service)
):
    """Получить детали с фильтрацией по артикулу и бренду"""
    result = await service.get_products(article=article, brand=brand)

    if result:
        return result

    raise HTTPException(status_code=404, detail="Item not found")
