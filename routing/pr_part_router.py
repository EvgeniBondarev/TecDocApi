from fastapi import APIRouter, Depends, Query, HTTPException
from typing import List, Optional
from dependencies import get_product_service
from schemas.pr_product_response_schema import PrProductResponseSchema

router = APIRouter(
    prefix="/pr-part",
    tags=["PrParts"]
)


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


@router.get("/by-td-id", response_model=List[PrProductResponseSchema])
async def get_parts_by_td_id(
    td_id: int = Query(...),
    article: Optional[str] = Query(None),
    service=Depends(get_product_service)
):
    """Получить детали по TdId через таблицу pr_PRODUCT_LINK, с фильтрацией по артикулу"""
    links = await service.get_links_by_td_id(td_id)
    if not links:
        raise HTTPException(status_code=404, detail="Brand not found by TdId")

    brand = links[0]["Brand"]
    result = await service.get_products(article=article, brand=brand)

    if result:
        return result

    raise HTTPException(status_code=404, detail="Items not found for brand and article")


@router.get("/by-jc-id", response_model=List[PrProductResponseSchema])
async def get_parts_by_jc_id(
    jc_id: int = Query(...),
    article: Optional[str] = Query(None),
    service=Depends(get_product_service)
):
    """Получить детали по JcId через таблицу pr_PRODUCT_LINK, с фильтрацией по артикулу"""
    links = await service.get_links_by_jc_id(jc_id)
    if not links:
        raise HTTPException(status_code=404, detail="Brand not found by JcId")

    brand = links[0]["Brand"]
    result = await service.get_products(article=article, brand=brand)

    if result:
        return result

    raise HTTPException(status_code=404, detail="Items not found for brand and article")
