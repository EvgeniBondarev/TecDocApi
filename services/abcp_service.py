from typing import List, Dict, Optional

from abcp.abcp_client import ABCPClient
from schemas.abcp.advice_schema import AdviceSchema
from schemas.abcp.article_schema import ArticleSchema
from schemas.abcp.batch_search_result import BatchSearchResult
from schemas.abcp.tip_schema import TipSchema


class ABCPService:
    def __init__(self, client: ABCPClient):
        self.client = client

    async def search_articles(self, number: str, brand: str) -> List[ArticleSchema]:
        return await self.client.search_articles(number, brand)

    async def search_tips(self, number: str) -> List[TipSchema]:
        """Поиск подсказок по номеру детали"""
        return await self.client.search_tips(number)

    async def get_advices(
            self,
            brand: str,
            number: str,
            limit: int = 5
    ) -> List[AdviceSchema]:
        """
        Получение рекомендаций по бренду и номеру детали
        """
        return await self.client.get_advices(brand, number, limit)

    async def batch_search(
            self,
            search_items: List[Dict[str, str]],
            profile_id: Optional[int] = None
    ) -> List[BatchSearchResult]:
        """
        Пакетный поиск деталей (до 100 позиций за запрос)
        :param search_items: Список словарей вида [{"brand": "Febi", "number": "01089"}, ...]
        :param profile_id: Опциональный ID профиля для расчета цен
        :return: Список найденных деталей
        """
        if len(search_items) > 100:
            raise ValueError("Maximum 100 items per batch request")

        return await self.client.batch_search(search_items, profile_id)