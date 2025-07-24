import re
from typing import List, Optional

from sqlalchemy import func

from schemas.product_information_schema import ProductInformationSchema
from repositories.product_information_repository import ProductInformationRepository

class ProductInformationService:
    def __init__(self, repository: ProductInformationRepository) -> None:
        self.repository = repository

    async def get_all_products(self) -> List[ProductInformationSchema]:
        records = await self.repository.find_all()
        return [ProductInformationSchema.model_validate(record) for record in records]

    async def get_product_by_article(self, article_number: str) -> Optional[ProductInformationSchema]:
        filter_condition = self.repository.model.ARTICLE_NUMBER == article_number
        record = await self.repository.find_one(filter_condition=filter_condition)
        return ProductInformationSchema.model_validate(record) if record else None

    async def get_products_by_manufacturer(self, manufacturer: str) -> List[ProductInformationSchema]:
        filter_condition = self.repository.model.MANUFACTURER == manufacturer
        records = await self.repository.find(filter_condition=filter_condition)
        return [ProductInformationSchema.model_validate(record) for record in records]

    async def get_product_by_article_and_manufacturer(
            self,
            article_number: str,
            manufacturer: str
    ) -> Optional[ProductInformationSchema]:
        """Получить продукт по артикулу и производителю без учёта регистра и лишних символов"""

        # Удаляем все символы кроме букв и цифр
        def normalize(value: str) -> str:
            return re.sub(r'[^a-zA-Z0-9а-яА-ЯёЁ]', '', value).lower()

        normalized_article = normalize(article_number)
        normalized_manufacturer = normalize(manufacturer)

        model = self.repository.model

        # Фильтр: приведение к нижнему регистру + удаление символов на стороне SQL
        filter_condition = (
                                   func.lower(
                                       func.replace(func.replace(func.replace(model.ARTICLE_NUMBER, ' ', ''), '-', ''),
                                                    '.', '')) ==
                                   normalized_article
                           ) & (
                                   func.lower(
                                       func.replace(func.replace(func.replace(model.MANUFACTURER, ' ', ''), '-', ''),
                                                    '.', '')) ==
                                   normalized_manufacturer
                           )

        record = await self.repository.find_one(filter_condition=filter_condition)
        return ProductInformationSchema.model_validate(record) if record else None