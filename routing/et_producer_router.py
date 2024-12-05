from fastapi import APIRouter, Depends
from typing import List
from dependencies import get_et_producer_service  # Импортируем зависимость для сервиса
from schemas.et_producer_schema import EtProducerSchema
from schemas.response_schema import ResponseSchema

router = APIRouter(
    prefix="/et-producers",
    tags=["Et Producers"]
)

@router.get("/", response_model=ResponseSchema[List[EtProducerSchema]])
async def get_all_producers(service=Depends(get_et_producer_service)):
    """Получить все записи производителей."""
    try:
        producers = await service.get_all_producers()
        if not producers:
            return ResponseSchema(error=True, message="No producers found.", result=[])
        return ResponseSchema(error=False, message="Success", result=producers)
    except Exception as e:
        return ResponseSchema(error=True, message=f"An error occurred: {str(e)}", result=[])


@router.get("/{supplier_id}", response_model=ResponseSchema[List[EtProducerSchema]])
async def get_producer_by_supplier_id(supplier_id: str, service=Depends(get_et_producer_service)):
    """Получить производителей по supplier_id."""
    try:
        producers = await service.get_producer_by_filter(supplier_id)
        if not producers:
            return ResponseSchema(error=True, message="Producer not found.", result=[])
        return ResponseSchema(error=False, message="Success", result=producers)
    except Exception as e:
        return ResponseSchema(error=True, message=f"An error occurred: {str(e)}", result=[])


@router.get("/name/{name}", response_model=ResponseSchema[List[EtProducerSchema]])
async def get_producers_by_name(name: str, service=Depends(get_et_producer_service)):
    """Получить производителей по имени."""
    try:
        producers = await service.get_producers_by_name(name)
        if not producers:
            return ResponseSchema(error=True, message="No producers found with that name.", result=[])
        return ResponseSchema(error=False, message="Success", result=producers)
    except Exception as e:
        return ResponseSchema(error=True, message=f"An error occurred: {str(e)}", result=[])