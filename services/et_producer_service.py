from typing import List
from repositories.et_producer_repository import EtProducerRepository
from schemas.et_producer_schema import EtProducerSchema


class EtProducerService:
    def __init__(self, repository: EtProducerRepository) -> None:
        self.repository = repository

    async def get_all_producers(self) -> List[EtProducerSchema]:
        records = await self.repository.find_all()
        return [EtProducerSchema.model_validate(record) for record in records]

    async def get_producer_by_filter(self, supplier_id: str) -> List[EtProducerSchema]:
        filter_condition = self.repository.model.tecdocSupplierId == supplier_id
        records = await self.repository.find(filter_condition=filter_condition)
        return [EtProducerSchema.model_validate(record) for record in records]

    async def get_producers_by_name(self, name: str) -> EtProducerSchema:
        filter_condition = self.repository.model.name == name
        record = await self.repository.find_one(filter_condition=filter_condition)
        return EtProducerSchema.model_validate(record)  if record else None

    async def get_producer_by_id(self, id: int) -> EtProducerSchema:
        filter_condition = self.repository.model.id == id
        record = await self.repository.find_one(filter_condition=filter_condition)
        return EtProducerSchema.model_validate(record) if record else None

    async def get_unique_market_prefix(self) -> List[str]:
        records = await self.repository.get_distinct_market_prefixes()
        return records if records else []
