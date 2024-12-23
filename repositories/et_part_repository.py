from db.database_enum import DatabaseEnum
from models.JCEtalon.et_parts import EtPart
from repositories.ABC.sql_alchemy_read_repository import SQLAlchemyReadRepository


class EtPartRepository(SQLAlchemyReadRepository):
    model = EtPart
    database = DatabaseEnum.JCEtalon
