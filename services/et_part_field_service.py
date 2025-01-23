from typing import List, Optional

from repositories.et_part_field_repository import EtPartFieldRepository
from schemas.et_part_field_schema import EtPartFieldSchema


class EtPartFieldService:
    def __init__(self, repository: EtPartFieldRepository) -> None:
        self.repository = repository

    async def get_all_partfields(self) -> List[EtPartFieldSchema]:
        records = await self.repository.find_all()
        return [EtPartFieldSchema.model_validate(record) for record in records]

    async def get_partfields_by_producer(self, producer_id: str) -> List[EtPartFieldSchema]:
        filter_condition = self.repository.model.producerid == producer_id
        records = await self.repository.find(filter_condition=filter_condition)
        return [EtPartFieldSchema.model_validate(record) for record in records]

    async def get_partfield_by_fieldid(self, field_id: str) -> List[EtPartFieldSchema]:
        filter_condition = self.repository.model.fieldid == field_id
        records = await self.repository.find(filter_condition=filter_condition)
        return [EtPartFieldSchema.model_validate(record) for record in records]

    async def get_partfields_by_dataid(self, data_id: str) -> List[EtPartFieldSchema]:
        filter_condition = self.repository.model.dataid == data_id
        records = await self.repository.find(filter_condition=filter_condition)
        return [EtPartFieldSchema.model_validate(record) for record in records]

    async def get_partfields_by_id_and_producer(self, id: int, producer_id: int) -> EtPartFieldSchema:
        filter_condition = (
            (self.repository.model.producerid == producer_id) &
            (self.repository.model.id == id)
        )
        record = await self.repository.find_one(filter_condition=filter_condition)
        return EtPartFieldSchema.model_validate(record) if record else None
