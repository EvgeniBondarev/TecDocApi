from fastapi import APIRouter, Depends
from typing import List
from dependencies import get_et_producer_service  # Импортируем зависимость для сервиса
from schemas.et_producer_schema import EtProducerSchema
from schemas.response_schema import ResponseSchema

router = APIRouter(
    prefix="/et-producers",
    tags=["Et Producers"]
)

@router.get("/{supplier_id}", response_model=EtProducerSchema)
async def get_producer_by_supplier_id(supplier_id: str, service=Depends(get_et_producer_service)):
    """Получить производителей по supplier_id."""
    return  await service.get_producer_by_filter(supplier_id)


@router.get("/name/{name}", response_model=EtProducerSchema)
async def get_producers_by_name(name: str, service=Depends(get_et_producer_service)):
    """Получить производителей по имени."""
    return await service.get_producers_by_name(name)
