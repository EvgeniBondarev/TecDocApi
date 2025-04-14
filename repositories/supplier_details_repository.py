from db.database_enum import DatabaseEnum
from models.TD2018.supplier_details import SupplierDetails
from repositories.ABC.sql_alchemy_read_repository import SQLAlchemyReadRepository


class SupplierDetailsRepository(SQLAlchemyReadRepository):
    model = SupplierDetails
    database = DatabaseEnum.TD2018