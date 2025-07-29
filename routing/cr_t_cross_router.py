from fastapi import APIRouter, Depends
from typing import List
from dependencies import get_cr_t_cross_service, get_et_producer_service
from schemas.cr_t_cross_schema import CrTCrossSchema
from services.cr_t_cross_service import CrTCrossService
from services.et_producer_service import EtProducerService

router = APIRouter(
    prefix="/cr-t-cross",
    tags=["CrTCross"]
)

@router.get("/maincode/{maincode}", response_model=List[CrTCrossSchema])
async def get_by_maincode(maincode: str, service: CrTCrossService=Depends(get_cr_t_cross_service),
                          et_producer_service: EtProducerService =Depends(get_et_producer_service)):
    """Получить записи по cr_maincode."""
    crosses: List[CrTCrossSchema] = await service.get_by_maincode(maincode)
    for cross in crosses:
        cross.et_producer = await  et_producer_service.get_producer_by_id(int(cross.cr_cross))
    return crosses

@router.get("/bycode/{bycode}", response_model=List[CrTCrossSchema])
async def get_by_bycode(bycode: str, service: CrTCrossService=Depends(get_cr_t_cross_service),
                          et_producer_service: EtProducerService =Depends(get_et_producer_service)):
    """Получить записи по cr_bycode."""
    crosses: List[CrTCrossSchema] = await service.get_by_bycode(bycode)
    for cross in crosses:
        cross.et_producer = await  et_producer_service.get_producer_by_id(int(cross.cr_cross))
    return crosses

@router.get("/crosscode/{crosscode}", response_model=List[CrTCrossSchema])
async def get_by_crosscode(crosscode: str, service: CrTCrossService=Depends(get_cr_t_cross_service),
                          et_producer_service: EtProducerService =Depends(get_et_producer_service)):
    """Получить записи по cr_bycode."""
    crosses: List[CrTCrossSchema] = await service.get_by_crosscode(crosscode)
    for cross in crosses:
        cross.et_producer = await  et_producer_service.get_producer_by_id(int(cross.cr_cross))
    return crosses
