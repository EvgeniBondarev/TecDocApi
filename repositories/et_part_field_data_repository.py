from db.database_enum import DatabaseEnum
from models.JCEtalon.et_part_field_data import EtPartFieldData
from repositories.ABC.sql_alchemy_read_repository import SQLAlchemyReadRepository


class EtPartFieldDataRepository(SQLAlchemyReadRepository):
    model = EtPartFieldData
    database = DatabaseEnum.JCEtalon
