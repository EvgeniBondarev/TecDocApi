from typing import List
from repositories.et_part_repository import EtPartRepository
from schemas.et_part_schema import EtPartSchema


class EtPartService:
    def __init__(self, repository: EtPartRepository) -> None:
        self.repository = repository

    async def get_all_parts(self) -> List[EtPartSchema]:
        records = await self.repository.find_all()
        return [EtPartSchema.model_validate(record) for record in records]

    async def get_parts_by_producer(self, producer_id: int) -> List[EtPartSchema]:
        filter_condition = self.repository.model.producerId == producer_id
        records = await self.repository.find(filter_condition=filter_condition)
        return [EtPartSchema.model_validate(record) for record in records]

    async def get_part_by_code(self, code: str) -> List[EtPartSchema]:
        filter_condition = self.repository.model.code == code
        records = await self.repository.find(filter_condition=filter_condition)
        return [EtPartSchema.model_validate(record) for record in records]

    async def get_part_by_code_and_producer(self, code: str, producer_id: int) -> List[EtPartSchema]:
        filter_condition = ((self.repository.model.code == code) & (self.repository.model.producerId == producer_id))
        records = await self.repository.find(filter_condition=filter_condition)
        return [EtPartSchema.model_validate(record) for record in records]

