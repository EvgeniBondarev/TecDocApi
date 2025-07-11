import requests
import re
from typing import List


class DuckDuckGoImageSearch:
    """Сервис для поиска изображений через DuckDuckGo"""

    def __init__(self):
        self.base_headers = {
            'User-Agent': 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36',
            'Accept': 'text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8',
            'Accept-Language': 'en-US,en;q=0.5',
            'Referer': 'https://duckduckgo.com/',
        }

    def search_images(self, query: str, num_images: int = 5) -> List[str]:
        """Поиск изображений по запросу"""
        try:
            # Шаг 1: Получаем токен vqd
            vqd = self._get_vqd_token(query)
            if not vqd:
                return []

            # Шаг 2: Получаем изображения через API
            return self._get_images_from_api(query, vqd, num_images)

        except Exception as e:
            print(f"Ошибка при поиске изображений: {str(e)}")
            return []

    def _get_vqd_token(self, query: str) -> str:
        """Получаем токен vqd для API запросов"""
        search_url = "https://duckduckgo.com/"
        params = {'q': query, 'iax': 'images', 'ia': 'images'}

        response = requests.post(
            search_url,
            headers=self.base_headers,
            params=params
        )
        response.raise_for_status()

        # Ищем токен в ответе
        vqd = re.search(r'vqd=([\'"]?)([\d-]+)\1', response.text)
        return vqd.group(2) if vqd else None

    def _get_images_from_api(self, query: str, vqd: str, num_images: int) -> List[str]:
        """Получаем изображения через API DuckDuckGo"""
        api_url = "https://duckduckgo.com/i.js"
        params = {
            'l': 'wt-wt',
            'o': 'json',
            'q': query,
            'vqd': vqd,
            'f': ',,,',
            'p': '1',
            'v7exp': 'a',
        }

        headers = self.base_headers.copy()
        headers.update({
            'Accept': 'application/json, text/javascript, */*; q=0.01',
            'X-Requested-With': 'XMLHttpRequest',
        })

        response = requests.get(api_url, headers=headers, params=params)
        response.raise_for_status()

        data = response.json()
        return [img['image'] for img in data.get('results', [])[:num_images]]