from typing import List, Optional, Dict
from repositories.supplier_details_repository import SupplierDetailsRepository
from schemas.supplier_details_schema import SupplierDetailsSchema


class SupplierDetailsService:
    def __init__(self, repository: SupplierDetailsRepository) -> None:
        self.repository = repository

    async def get_all_supplier_details(self) -> List[SupplierDetailsSchema]:
        """Получить все записи модели."""
        records = await self.repository.find_all()
        return [SupplierDetailsSchema.model_validate(record) for record in records]

    async def get_details_by_supplier_id(self, supplier_id: int) -> List[SupplierDetailsSchema]:
        """Получить все записи по ID поставщика."""
        filter_condition = self.repository.model.supplierid == supplier_id
        records = await self.repository.find(filter_condition=filter_condition)
        return [SupplierDetailsSchema.model_validate(record) for record in records]

    async def get_detail_by_composite_id(self, supplier_id: int, addresstypeid: str) -> Optional[SupplierDetailsSchema]:
        """Получить запись по составному ID (supplierid + addresstypeid)."""
        filter_condition = (
                (self.repository.model.supplierid == supplier_id) &
                (self.repository.model.addresstypeid == addresstypeid)
        )
        record = await self.repository.find_one(filter_condition=filter_condition)
        return SupplierDetailsSchema.model_validate(record) if record else None

    async def get_details_by_address_type(self, addresstype: str) -> List[SupplierDetailsSchema]:
        """Получить записи по типу адреса."""
        filter_condition = self.repository.model.addresstype.like(f"%{addresstype}%")
        records = await self.repository.find(filter_condition=filter_condition)
        return [SupplierDetailsSchema.model_validate(record) for record in records]

    async def get_details_by_country(self, countrycode: str) -> List[SupplierDetailsSchema]:
        """Получить записи по коду страны."""
        filter_condition = self.repository.model.countrycode == countrycode
        records = await self.repository.find(filter_condition=filter_condition)
        return [SupplierDetailsSchema.model_validate(record) for record in records]

    async def get_details_by_city(self, city: str) -> List[SupplierDetailsSchema]:
        """Получить записи по городу (ищет в city1 и city2)."""
        filter_condition = (
                (self.repository.model.city1.like(f"%{city}%")) |
                (self.repository.model.city2.like(f"%{city}%"))
        )
        records = await self.repository.find(filter_condition=filter_condition)
        return [SupplierDetailsSchema.model_validate(record) for record in records]

    async def get_suppliers_by_email_domain(self, domain: str) -> List[SupplierDetailsSchema]:
        """Получить записи по домену email."""
        filter_condition = self.repository.model.email.like(f"%@{domain}%")
        records = await self.repository.find(filter_condition=filter_condition)
        return [SupplierDetailsSchema.model_validate(record) for record in records]

    async def get_details_by_supplier_ids(self, supplier_ids: List[int]) -> Dict[int, List[SupplierDetailsSchema]]:
        """Получить детали для списка supplier_ids, сгруппированные по supplierid."""
        filter_condition = self.repository.model.supplierid.in_(supplier_ids)
        records = await self.repository.find(filter_condition=filter_condition)

        result = {}
        for record in records:
            schema = SupplierDetailsSchema.model_validate(record)
            if schema.supplierid not in result:
                result[schema.supplierid] = []
            result[schema.supplierid].append(schema)

        return result