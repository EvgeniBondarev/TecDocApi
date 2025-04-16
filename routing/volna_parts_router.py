from typing import List

from fastapi import APIRouter, Depends

from dependencies import get_volna_parts_parser
from schemas.part_data_shema import PartDataSchema
from schemas.volva.volna_part import VolnaPart
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

@router.get("/part-details/{article}", response_model=List[VolnaPart])
async def get_part_details(article: str, brand: int = None, service: VolnaPartsParser = Depends(get_volna_parts_parser)):
    """Get detailed part information with attributes as a list of models"""
    return service.parse_part_details(article, brand)

@router.get("/part-details/{article}/{brand}", response_model=List[VolnaPart])
async def get_part_details_with_brand(article: str, brand: int, service: VolnaPartsParser = Depends(get_volna_parts_parser)):
    """Get detailed part information with attributes as a list of models for specific brand"""
    return service.parse_part_details(article, brand)