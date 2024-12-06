from typing import List, Optional
from repositories.suppliers_repository import SuppliersRepository
from schemas.suppliers_schema import SuppliersSchema


class SuppliersService:
    def __init__(self, repository: SuppliersRepository) -> None:
        self.repository = repository

    async def get_all_suppliers(self) -> List[SuppliersSchema]:
        """Получить все записи модели."""
        records = await self.repository.find_all()
        return [SuppliersSchema.model_validate(record) for record in records]

    async def get_supplier_by_id(self, supplier_id: int) -> Optional[SuppliersSchema]:
        """Получить запись по ID."""
        record = await self.repository.find_one(filter_condition=self.repository.model.id == supplier_id)
        return SuppliersSchema.model_validate(record) if record else None

    async def get_suppliers_by_description(self, description: str) -> List[SuppliersSchema]:
        """Получить записи по описанию (учитывая регистр)."""
        filter_condition = self.repository.model.description.like(f"%{description}%")
        records = await self.repository.find(filter_condition=filter_condition)
        return [SuppliersSchema.model_validate(record) for record in records]

    async def get_suppliers_by_description_case_ignore(self, description: str) -> SuppliersSchema:
        """Получить записи по описанию (без учета регистра)."""
        filter_condition = self.repository.model.description.ilike(f"%{description}%")
        record = await self.repository.find_one(filter_condition=filter_condition)
        return SuppliersSchema.model_validate(record) if record else None

    async def get_suppliers_by_matchcode(self, matchcode: str) -> SuppliersSchema:
        """Получить записи по описанию (без учета регистра)."""
        filter_condition = self.repository.model.matchcode == matchcode
        record = await self.repository.find_one(filter_condition=filter_condition)
        return SuppliersSchema.model_validate(record)

    async def get_suppliers_with_articles(self) -> List[SuppliersSchema]:
        """Получить поставщиков, у которых есть статьи."""
        filter_condition = self.repository.model.nbrofarticles > 0
        records = await self.repository.find(filter_condition=filter_condition)
        return [SuppliersSchema.model_validate(record) for record in records]

    async def get_suppliers_with_new_version_articles(self) -> List[SuppliersSchema]:
        """Получить поставщиков, у которых есть статьи новой версии."""
        filter_condition = self.repository.model.hasnewversionarticles == "True"
        records = await self.repository.find(filter_condition=filter_condition)
        return [SuppliersSchema.model_validate(record) for record in records]
