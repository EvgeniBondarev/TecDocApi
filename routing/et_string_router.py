from fastapi import APIRouter, Depends, HTTPException
from typing import List, Optional
from services.et_string_service import EtStringService
from schemas.et_string_schema import EtStringSchema
from dependencies import get_et_string_service

router = APIRouter(
    prefix="/et-string",
    tags=["EtString"],
)


@router.get("/", response_model=List[EtStringSchema])
async def get_all_strings(service: EtStringService = Depends(get_et_string_service)) -> List[EtStringSchema]:
    """
    Получить все строки.
    """
    return await service.get_all_strings()


@router.get("/{id}", response_model=EtStringSchema)
async def get_string_by_id(id: int, service: EtStringService = Depends(get_et_string_service)) -> EtStringSchema:
    """
    Получить строку по ID.
    """
    result = await service.get_string_by_id(id)
    if not result:
        raise HTTPException(status_code=404, detail="String not found")
    return result


@router.get("/producer/{producer_id}", response_model=List[EtStringSchema])
async def get_strings_by_producer(producer_id: int, service: EtStringService = Depends(get_et_string_service)) -> List[EtStringSchema]:
    """
    Получить строки по producerId.
    """
    return await service.get_strings_by_producer(producer_id)


@router.get("/language/{lng}", response_model=List[EtStringSchema])
async def get_strings_by_language(lng: int, service: EtStringService = Depends(get_et_string_service)) -> List[EtStringSchema]:
    """
    Получить строки по языку (lng).
    """
    return await service.get_strings_by_language(lng)


@router.get("/search/", response_model=List[EtStringSchema])
async def search_strings_by_text(
    text_substring: str, service: EtStringService = Depends(get_et_string_service)
) -> List[EtStringSchema]:
    """
    Поиск строк по подстроке в поле text.
    """
    return await service.search_strings_by_text(text_substring)

@router.get("/{id_str}/{producer_id}", response_model=EtStringSchema)
async def search_strings_by_text(
    id_str: int,
    producer_id: int,
    service: EtStringService = Depends(get_et_string_service)
) -> List[EtStringSchema]:
    return await service.search_strings_by_id_str_and_producer_id(id_str, producer_id)