from db.database_enum import DatabaseEnum
from models.TD2018.suppliers import Suppliers
from repositories.ABC.sql_alchemy_read_repository import SQLAlchemyReadRepository


class SuppliersRepository(SQLAlchemyReadRepository):
    model = Suppliers
    database = DatabaseEnum.TD2018