from db.database_enum import DatabaseEnum
from models.JCCross.cr_t_cross import CrTCross
from repositories.ABC.sql_alchemy_read_repository import SQLAlchemyReadRepository

class CrTCrossRepository(SQLAlchemyReadRepository):
    model = CrTCross
    database = DatabaseEnum.JCCross