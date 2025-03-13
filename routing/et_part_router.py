from fastapi import APIRouter, Depends
from typing import List, Optional

from dependencies import get_et_part_service
from schemas.et_part_schema import EtPartSchema

router = APIRouter(
    prefix="/et-part",
    tags=["EtParts"]
)


@router.get("/", response_model=List[EtPartSchema])
async def get_all_parts(service=Depends(get_et_part_service)):
    """Получить все детали."""
    return await service.get_all_parts()


@router.get("/producer/{producer_id}", response_model=List[EtPartSchema])
async def get_parts_by_producer(producer_id: int, service=Depends(get_et_part_service)):
    """Получить детали по идентификатору производителя."""
    return await service.get_parts_by_producer(producer_id)


@router.get("/code/{code}", response_model=List[EtPartSchema])
async def get_part_by_code(code: str, service=Depends(get_et_part_service)):
    """Получить детали по коду."""
    return await service.get_part_by_code(code)

@router.get("/{code}/{producer_id}/", response_model=List[EtPartSchema])
async def get_part_by_code_and_producer(code: str, producer_id: int, service=Depends(get_et_part_service)):
    """Получить детали по коду и идентификатору производителя."""
    return await service.get_part_by_code_and_producer(code, producer_id)
