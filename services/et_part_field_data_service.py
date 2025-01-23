from typing import List, Optional

from repositories.et_part_field_data_repository import EtPartFieldDataRepository
from schemas.et_part_field_data_schema import EtPartFieldDataSchema


class EtPartFieldDataService:
    def __init__(self, repository: EtPartFieldDataRepository) -> None:
        self.repository = repository

    async def get_all_data(self) -> List[EtPartFieldDataSchema]:
        records = await self.repository.find_all()
        return [EtPartFieldDataSchema.model_validate(record) for record in records]

    async def get_data_by_id(self, id: int) -> EtPartFieldDataSchema:
        filter_condition = self.repository.model.id == id
        record = await self.repository.find_one(filter_condition=filter_condition)
        return EtPartFieldDataSchema.model_validate(record) if record else None

    async def get_data_by_value(self, data_value: str) -> List[EtPartFieldDataSchema]:
        filter_condition = self.repository.model.data == data_value
        records = await self.repository.find(filter_condition=filter_condition)
        return [EtPartFieldDataSchema.model_validate(record) for record in records]
