from typing import List, Optional

from repositories.article_attributes_repository import ArticlesAttributesRepository
from schemas.article_attributes_schema import ArticleAttributesSchema


class ArticleAttributesService:
    def __init__(self, repository: ArticlesAttributesRepository) -> None:
        self.repository = repository

    async def get_all_attributes(self) -> List[ArticleAttributesSchema]:
        """Получить все записи модели."""
        records = await self.repository.find_all()
        return [ArticleAttributesSchema.model_validate(record) for record in records]

    async def get_attribute_by_filter(self, supplier_id: int, article: str) ->  List[ArticleAttributesSchema]:
        """Получить одну запись по фильтру."""
        filter_condition = (
            (self.repository.model.supplierid == supplier_id) &
            (self.repository.model.datasupplierarticlenumber == article)
        )
        records = await self.repository.find(filter_condition=filter_condition)
        return [ArticleAttributesSchema.model_validate(record) for record in records]

    async def get_attributes_by_supplier(self, supplier_id: int) -> List[ArticleAttributesSchema]:
        """Получить все атрибуты для заданного поставщика."""
        filter_condition = self.repository.model.supplierid == supplier_id
        records = await self.repository.find(filter_condition=filter_condition)
        return [ArticleAttributesSchema.model_validate(record) for record in records]