from typing import List
from repositories.article_ean_repository import ArticleEANRepository
from schemas.article_ean_schema import ArticleEANSchema


class ArticleEANService:
    def __init__(self, repository: ArticleEANRepository) -> None:
        self.repository = repository

    async def get_all_eans(self) -> List[ArticleEANSchema]:
        """Получить все записи модели."""
        records = await self.repository.find_all()
        return [ArticleEANSchema.model_validate(record) for record in records]

    async def get_ean_by_filter(self, supplier_id: int, article: str) -> ArticleEANSchema:
        """Получить записи по фильтру."""
        filter_condition = (
            (self.repository.model.supplierid == supplier_id) &
            (self.repository.model.datasupplierarticlenumber == article)
        )
        record = await self.repository.find_one(filter_condition=filter_condition)
        return ArticleEANSchema.model_validate(record) if record else None

    async def get_eans_by_supplier(self, supplier_id: int) -> List[ArticleEANSchema]:
        """Получить все EAN для заданного поставщика."""
        filter_condition = self.repository.model.supplierid == supplier_id
        records = await self.repository.find(filter_condition=filter_condition)
        return [ArticleEANSchema.model_validate(record) for record in records]
