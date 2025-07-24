from typing import List, Optional
from repositories.article_cross_repository import ArticleCrossRepository
from schemas.article_cross_schema import ArticleCrossSchema

class ArticleCrossService:
    def __init__(self, repository: ArticleCrossRepository) -> None:
        self.repository = repository

    async def get_all_articles(self) -> List[ArticleCrossSchema]:
        """Получить все записи модели."""
        records = await self.repository.find_all()
        return [ArticleCrossSchema.model_validate(record) for record in records]

    async def get_article_by_filter(
        self, manufacturer_id: int, oe_nbr: str, supplier_id: int, article_number: str
    ) -> Optional[ArticleCrossSchema]:
        """Получить запись по фильтру."""
        filter_condition = (
            (self.repository.model.manufacturerId == manufacturer_id) &
            (self.repository.model.OENbr == oe_nbr) &
            (self.repository.model.SupplierId == supplier_id) &
            (self.repository.model.PartsDataSupplierArticleNumber == article_number)
        )
        record = await self.repository.find_one(filter_condition=filter_condition)
        return ArticleCrossSchema.model_validate(record) if record else None

    async def get_articles_by_manufacturer(self, manufacturer_id: int) -> List[ArticleCrossSchema]:
        """Получить все записи для заданного производителя."""
        filter_condition = self.repository.model.manufacturerId == manufacturer_id
        records = await self.repository.find(filter_condition=filter_condition)
        return [ArticleCrossSchema.model_validate(record) for record in records]

    async def get_articles_by_supplier(self, supplier_id: int) -> List[ArticleCrossSchema]:
        """Получить все записи для заданного поставщика."""
        filter_condition = self.repository.model.SupplierId == supplier_id
        records = await self.repository.find(filter_condition=filter_condition)
        return [ArticleCrossSchema.model_validate(record) for record in records]

    async def get_articles_by_article(self, article: str) -> List[ArticleCrossSchema]:
        """Получить все записи для заданного поставщика."""
        filter_condition = self.repository.model.PartsDataSupplierArticleNumber == article
        records = await self.repository.find(filter_condition=filter_condition)
        return [ArticleCrossSchema.model_validate(record) for record in records]

    async def get_article_by_article_and_supplier(
        self, supplier_id: int, article_number: str
    ) -> Optional[ArticleCrossSchema]:
        """Получить запись по фильтру."""
        filter_condition = (
            (self.repository.model.SupplierId == supplier_id) &
            (self.repository.model.PartsDataSupplierArticleNumber == article_number)
        )
        record = await self.repository.find_one(filter_condition=filter_condition)
        return ArticleCrossSchema.model_validate(record) if record else None