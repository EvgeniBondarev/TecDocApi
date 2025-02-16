from fastapi import APIRouter, Depends
from typing import List
from dependencies import get_cr_t_cross_service
from schemas.cr_t_cross_schema import CrTCrossSchema

router = APIRouter(
    prefix="/cr-t-cross",
    tags=["CrTCross"]
)

@router.get("/maincode/{maincode}", response_model=List[CrTCrossSchema])
async def get_by_maincode(maincode: str, service=Depends(get_cr_t_cross_service)):
    """Получить записи по cr_maincode."""
    return await service.get_by_maincode(maincode)
