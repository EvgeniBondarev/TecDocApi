from typing import List, Optional, Set

from repositories.manufacturer_repository import ManufacturerRepository
from schemas.manufacturer_schema import ManufacturerSchema


class ManufacturerService:
    def __init__(self, repository: ManufacturerRepository) -> None:
        self.repository = repository

    async def get_all_manufacturers(self) -> List[ManufacturerSchema]:
        """Получить все записи модели."""
        records = await self.repository.find_all()
        return [ManufacturerSchema.model_validate(record) for record in records]

    async def get_manufacturer_by_id(
        self, id: int) -> Optional[ManufacturerSchema]:
        """Получить запись по составному первичному ключу."""
        filter_condition = self.repository.model.id == id
        record = await self.repository.find_one(filter_condition=filter_condition)
        return ManufacturerSchema.model_validate(record) if record else None

    async def get_manufacturers_by_ids(self, manufacturer_ids: Set[int]) -> List[ManufacturerSchema]:
        """Получить записи по списку ID."""
        filter_condition = self.repository.model.id.in_(manufacturer_ids)
        records = await self.repository.find(filter_condition=filter_condition)
        return [ManufacturerSchema.model_validate(record) for record in records]

    async def get_manufacturers_by_display_status(self, canbedisplayed: str) -> List[ManufacturerSchema]:
        """Получить все записи по статусу отображения."""
        filter_condition = self.repository.model.canbedisplayed == canbedisplayed
        records = await self.repository.find(filter_condition=filter_condition)
        return [ManufacturerSchema.model_validate(record) for record in records]