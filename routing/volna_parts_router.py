from typing import List

from fastapi import APIRouter, Depends

from dependencies import get_volna_parts_parser
from schemas.part_data_shema import PartDataSchema
from services.volna_parts_parser import VolnaPartsParser

router = APIRouter(
    prefix="/volna-parts",
    tags=["VolnaPrts"]
)

@router.get("/part/{article}", response_model=List[PartDataSchema])
async def get_by_article(article: str, service: VolnaPartsParser=Depends(get_volna_parts_parser)):
    """Получить записи по article."""
    return  service.parse_part(article)

@router.get("/part/{article}/{brand}", response_model=List[PartDataSchema])
async def get_by_article_and_brand(article: str, brand: int, service: VolnaPartsParser=Depends(get_volna_parts_parser)):
    """Получить записи по article."""
    return  service.parse_part_by_brand(article, brand)