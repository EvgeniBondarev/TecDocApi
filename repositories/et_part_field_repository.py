from db.database_enum import DatabaseEnum
from models.JCEtalon.et_part_field import EtPartField
from repositories.ABC.sql_alchemy_read_repository import SQLAlchemyReadRepository


class EtPartFieldRepository(SQLAlchemyReadRepository):
    model = EtPartField
    database = DatabaseEnum.JCEtalon
