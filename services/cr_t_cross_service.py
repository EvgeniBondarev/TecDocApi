from typing import List, Optional
from repositories.cr_t_cross_repository import CrTCrossRepository
from schemas.cr_t_cross_schema import CrTCrossSchema

class CrTCrossService:
    def __init__(self, repository: CrTCrossRepository) -> None:
        self.repository = repository

    async def get_by_maincode(self, maincode: str) -> List[CrTCrossSchema]:
        filter_condition = self.repository.model.cr_maincode == maincode
        records = await self.repository.find(filter_condition=filter_condition)
        return [CrTCrossSchema.model_validate(record) for record in records]

    async def get_by_bycode(self, bycode: str) -> List[CrTCrossSchema]:
        filter_condition = self.repository.model.cr_bycode == bycode
        records = await self.repository.find(filter_condition=filter_condition)
        return [CrTCrossSchema.model_validate(record) for record in records]
