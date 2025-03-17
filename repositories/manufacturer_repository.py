from db.database_enum import DatabaseEnum
from models.TD2018.manufacturer import Manufacturer
from repositories.ABC.sql_alchemy_read_repository import SQLAlchemyReadRepository


class ManufacturerRepository(SQLAlchemyReadRepository):
    model = Manufacturer 
    database = DatabaseEnum.TD2018