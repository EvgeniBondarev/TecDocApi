from fastapi import APIRouter, Depends
from typing import List, Optional

from dependencies import get_abcp_service
from schemas.abcp.advice_schema import AdviceSchema
from schemas.abcp.article_schema import ArticleSchema
from schemas.abcp.batch_search_request import BatchSearchRequest
from schemas.abcp.batch_search_result import BatchSearchResult
from schemas.abcp.tip_schema import TipSchema

router = APIRouter(
    prefix="/abcp",
    tags=["ABCP API"]
)

@router.get("/search/", response_model=List[ArticleSchema])
async def search_articles(
    number: str,
    brand: str,
    service=Depends(get_abcp_service)
):
    """Поиск артикулов в API ABCP"""
    return await service.search_articles(number, brand)

@router.get("/tips/", response_model=List[TipSchema])
async def search_tips(
    number: str,
    service=Depends(get_abcp_service)
):
    """Поиск подсказок по номеру детали в API ABCP"""
    return await service.search_tips(number)


@router.get("/advices/", response_model=List[AdviceSchema])
async def get_advices(
        brand: str,
        number: str,
        limit: int = 5,
        service=Depends(get_abcp_service)
):
    """
    Получение рекомендаций по бренду и номеру детали из API ABCP

    Параметры:
    - brand: Бренд для поиска (например, 'MB')
    - number: Номер детали (например, 'A2058350147')
    - limit: Лимит результатов (по умолчанию 5)
    """
    return await service.get_advices(brand, number, limit)


@router.get("/search/", response_model=List[BatchSearchResult])
async def search_article(
        brand: str,
        number: str,
        profile_id: Optional[int] = None,
        service=Depends(get_abcp_service)
):
    """
    Поиск детали по бренду и номеру без учета аналогов

    Параметры:
    - brand: Бренд производителя (например, "Febi")
    - number: Номер детали (например, "01089")
    - profile_id: Опциональный ID профиля для расчета цен

    Пример запроса:
    /search/?brand=Febi&number=01089
    """
    return await service.batch_search(
        [{"brand": brand, "number": number}],
        profile_id
    )


@router.post("/batch-search/", response_model=List[BatchSearchResult])
async def batch_search(
        request: BatchSearchRequest,
        service=Depends(get_abcp_service)
):
    """
    Пакетный поиск деталей без учета аналогов (до 100 позиций за запрос)

    Пример тела запроса:
    {
        "search": [
            {"brand": "Febi", "number": "01089"},
            {"brand": "Kyb", "number": "333305"}
        ],
        "profileId": 12345
    }
    """
    return await service.batch_search(
        [item.dict() for item in request.search],
        request.profileId
    )