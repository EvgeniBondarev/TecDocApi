from fastapi import APIRouter, Depends
from typing import List, Optional
from dependencies import get_product_information_service
from schemas.product_information_schema import ProductInformationSchema

router = APIRouter(
    prefix="/product-information",
    tags=["Product Information"]
)

from fastapi import Query

@router.get("/product/", response_model=Optional[ProductInformationSchema])
async def get_product(
    article_number: str = Query(None, description="Номер артикула"),
    manufacturer: str = Query(None, description="Производитель"),
    service=Depends(get_product_information_service)
):
    """Получить продукт по различным параметрам"""
    if article_number and manufacturer:
        return await service.get_product_by_article_and_manufacturer(article_number, manufacturer)
    elif article_number:
        return await service.get_product_by_article(article_number)
    elif manufacturer:
        products = await service.get_products_by_manufacturer(manufacturer)
        return products[0] if products else None
    return None