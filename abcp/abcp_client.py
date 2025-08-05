import json

import httpx
from typing import List, Dict, Optional

from abcp.abcp_urls import ABCPUrls
from schemas.abcp.advice_schema import AdviceSchema
from schemas.abcp.article_schema import ArticleSchema
from schemas.abcp.batch_search_result import BatchSearchResult
from schemas.abcp.tip_schema import TipSchema


class ABCPClient:
    def __init__(self, base_url: str, user_login: str, user_psw: str):
        self.base_url = base_url
        self.user_login = user_login
        self.user_psw = user_psw
        self.client = httpx.AsyncClient()

    async def search_articles(self, number: str, brand: str) -> List[ArticleSchema]:
        params = {
            "userlogin": self.user_login,
            "userpsw": self.user_psw,
            "number": number,
            "brand": brand
        }
        response = await self.client.get(
            f"{self.base_url}{ABCPUrls.SEARCH_ARTICLES}",
            params=params
        )
        response.raise_for_status()
        return [ArticleSchema(**item) for item in response.json()]

    async def search_tips(self, number: str) -> List[TipSchema]:
        """Поиск подсказок по номеру детали"""
        params = {
            "userlogin": self.user_login,
            "userpsw": self.user_psw,
            "number": number
        }
        response = await self.client.get(
            f"{self.base_url}{ABCPUrls.SEARCH_TIPS}",
            params=params
        )
        response.raise_for_status()
        return [TipSchema(**item) for item in response.json()]

    async def batch_search(
            self,
            search_items: List[Dict[str, str]],
            profile_id: Optional[int] = None
    ) -> List[BatchSearchResult]:
        """
        Пакетный поиск деталей без учета аналогов
        :param search_items: Список словарей с ключами 'brand' и 'number'
        :param profile_id: ID профиля для расчета цен
        :return: Список найденных деталей
        """
        data = {
            "userlogin": self.user_login,
            "userpsw": self.user_psw,
            **({"profileId": profile_id} if profile_id else {})
        }

        # Добавляем элементы поиска
        for idx, item in enumerate(search_items):
            data[f"search[{idx}][brand]"] = item["brand"]
            data[f"search[{idx}][number]"] = item["number"]

        try:
            response = await self.client.post(
                f"{self.base_url}{ABCPUrls.SEARCH_BATCH}",
                data=data,
                timeout=30.0
            )
            response.raise_for_status()

            # Дополнительная обработка ответа перед валидацией
            response_data = response.json()
            if not isinstance(response_data, list):
                raise Exception("Invalid response format from API")

            results = []
            for item in response_data:
                try:
                    # Преобразуем supplierColor в строку, если он пришел как число
                    if 'supplierColor' in item and isinstance(item['supplierColor'], int):
                        item['supplierColor'] = str(item['supplierColor'])

                    # Обрабатываем пустые строки в числовых полях
                    for field in ['deliveryPeriodMax', 'volume', 'priceIn', 'priceRate']:
                        if field in item and item[field] == "":
                            item[field] = None

                    results.append(BatchSearchResult(**item))
                except Exception as e:
                    # Логируем ошибку, но продолжаем обработку остальных элементов
                    print(f"Error processing item {item.get('brand')} {item.get('number')}: {str(e)}")
                    continue

            return results

        except httpx.HTTPStatusError as e:
            raise Exception(f"API error: {e.response.status_code} {e.response.text}")
        except json.JSONDecodeError:
            raise Exception("Invalid JSON response from API")
        except Exception as e:
            raise Exception(f"Request failed: {str(e)}")