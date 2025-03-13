from db.database_enum import DatabaseEnum
from models.JCEtalon.et_string import EtString
from repositories.ABC.sql_alchemy_read_repository import SQLAlchemyReadRepository


class EtStringRepository(SQLAlchemyReadRepository):
    model = EtString
    database = DatabaseEnum.JCEtalon
