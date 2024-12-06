from typing import List, Optional
from repositories.articles_repository import ArticlesRepository
from schemas.articles_schema import ArticlesSchema


class ArticlesService:
    def __init__(self, repository: ArticlesRepository) -> None:
        self.repository = repository

    async def get_all_articles(self) -> List[ArticlesSchema]:
        """Получить все записи модели."""
        records = await self.repository.find_all()
        return [ArticlesSchema.model_validate(record) for record in records]

    async def get_article_by_id(self, supplier_id: int, article_number: str) -> Optional[ArticlesSchema]:
        """Получить запись по идентификатору (supplierId + DataSupplierArticleNumber)."""
        filter_condition = (
            (self.repository.model.supplierId == supplier_id) &
            (self.repository.model.DataSupplierArticleNumber == article_number)
        )
        record = await self.repository.find_one(filter_condition=filter_condition)
        return ArticlesSchema.model_validate(record) if record else None

    async def get_articles_by_supplier(self, supplier_id: int) -> List[ArticlesSchema]:
        """Получить все записи для заданного поставщика (supplierId)."""
        filter_condition = self.repository.model.supplierId == supplier_id
        records = await self.repository.find(filter_condition=filter_condition)
        return [ArticlesSchema.model_validate(record) for record in records]

    async def get_articles_by_found_string(self, found_string: str) -> ArticlesSchema:
        """Получить записи по полю FoundString."""
        filter_condition = self.repository.model.FoundString == found_string
        record = await self.repository.find_one(filter_condition=filter_condition)
        return ArticlesSchema.model_validate(record)

    async def get_valid_articles(self) -> List[ArticlesSchema]:
        """Получить только валидные статьи (IsValid = 'True')."""
        filter_condition = self.repository.model.IsValid == "True"
        records = await self.repository.find(filter_condition=filter_condition)
        return [ArticlesSchema.model_validate(record) for record in records]

    async def get_articles_with_packing_info(self) -> List[ArticlesSchema]:
        """Получить статьи с информацией о единицах упаковки."""
        filter_condition = (
            (self.repository.model.PackingUnit.isnot(None)) &
            (self.repository.model.QuantityPerPackingUnit.isnot(None))
        )
        records = await self.repository.find(filter_condition=filter_condition)
        return [ArticlesSchema.model_validate(record) for record in records]
