from typing import List, Optional, Tuple

from repositories.article_oe_repository import ArticleOERepository
from schemas.article_oe_schema import ArticleOESchema


class ArticleOEService:
    def __init__(self, repository: ArticleOERepository) -> None:
        self.repository = repository

    async def get_all_articles(self) -> List[ArticleOESchema]:
        """Получить все записи модели."""
        records = await self.repository.find_all()
        return [ArticleOESchema.model_validate(record) for record in records]

    async def get_article_by_filter(
        self, supplierid: int, datasupplierarticlenumber: str
    ) -> Optional[ArticleOESchema]:
        """Получить запись по фильтру."""
        filter_condition = (
            (self.repository.model.supplierid == supplierid) &
            (self.repository.model.datasupplierarticlenumber == datasupplierarticlenumber)
        )
        record = await self.repository.find_one(filter_condition=filter_condition)
        return ArticleOESchema.model_validate(record) if record else None


    async def get_article_by_oen(
        self, supplierid: int, oen_br: str
    ) -> List[ArticleOESchema]:
        """Получить запись по OENbr."""
        filter_condition = (
            (self.repository.model.supplierid == supplierid) &
            (self.repository.model.OENbr == oen_br)
        )
        records = await self.repository.find(filter_condition=filter_condition)
        return [ArticleOESchema.model_validate(record) for record in records]

    async def get_articles_by_manufacturer(self, manufacturerId: int) -> List[ArticleOESchema]:
        """Получить все записи для заданного производителя."""
        filter_condition = self.repository.model.manufacturerId == manufacturerId
        records = await self.repository.find(filter_condition=filter_condition)
        return [ArticleOESchema.model_validate(record) for record in records]

    async def get_articles_by_oe_number(self, OENbr: str) -> List[ArticleOESchema]:
        """Получить все записи для заданного OE номера."""
        filter_condition = self.repository.model.OENbr == OENbr
        records = await self.repository.find(filter_condition=filter_condition)
        return [ArticleOESchema.model_validate(record) for record in records]

    async def get_articles_by_oe_article(self, article: str) -> List[ArticleOESchema]:
        """Получить все записи для заданного OE номера."""
        filter_condition = self.repository.model.datasupplierarticlenumber == article
        records = await self.repository.find(filter_condition=filter_condition)
        return [ArticleOESchema.model_validate(record) for record in records]


    async def get_articles_by_oen_supplier_pairs(self, oen_supplier_pairs: List[Tuple[str, int]]) -> List[
        ArticleOESchema]:
        """Получить записи по списку пар (OENbr, SupplierId)."""
        filter_conditions = [
            (self.repository.model.OENbr == oen) & (self.repository.model.supplierid == supplier_id)
            for oen, supplier_id in oen_supplier_pairs
        ]
        combined_filter = filter_conditions[0]
        for condition in filter_conditions[1:]:
            combined_filter |= condition

        records = await self.repository.find(filter_condition=combined_filter)
        return [ArticleOESchema.model_validate(record) for record in records]