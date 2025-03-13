from typing import List, Optional
from repositories.et_string_repository import EtStringRepository
from schemas.et_string_schema import EtStringSchema


class EtStringService:
    def __init__(self, repository: EtStringRepository) -> None:
        self.repository = repository

    async def get_all_strings(self) -> List[EtStringSchema]:
        records = await self.repository.find_all()
        return [EtStringSchema.model_validate(record) for record in records]

    async def get_string_by_id(self, id: int) -> Optional[EtStringSchema]:
        filter_condition = self.repository.model.id == id
        record = await self.repository.find_one(filter_condition=filter_condition)
        return EtStringSchema.model_validate(record) if record else None

    async def get_strings_by_producer(self, producer_id: int) -> List[EtStringSchema]:
        filter_condition = self.repository.model.producerId == producer_id
        records = await self.repository.find(filter_condition=filter_condition)
        return [EtStringSchema.model_validate(record) for record in records]

    async def get_strings_by_language(self, lng: int) -> List[EtStringSchema]:
        filter_condition = self.repository.model.lng == lng
        records = await self.repository.find(filter_condition=filter_condition)
        return [EtStringSchema.model_validate(record) for record in records]

    async def search_strings_by_text(self, text_substring: str) -> List[EtStringSchema]:
        filter_condition = self.repository.model.text.contains(text_substring)
        records = await self.repository.find(filter_condition=filter_condition)
        return [EtStringSchema.model_validate(record) for record in records]

    async def search_strings_by_id_str_and_producer_id(self, id_str: int, producer_id: int) -> List[EtStringSchema]:
        filter_condition = (
            (self.repository.model.idstr == id_str) &
            (self.repository.model.producerId == producer_id)
        )
        record = await self.repository.find_one(filter_condition=filter_condition)
        return EtStringSchema.model_validate(record) if record else None
