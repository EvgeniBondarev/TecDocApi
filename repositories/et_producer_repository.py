from db.database_enum import DatabaseEnum
from models.JCEtalon.et_producers import EtProducer
from repositories.ABC.sql_alchemy_read_repository import SQLAlchemyReadRepository


class EtProducerRepository(SQLAlchemyReadRepository):
    model = EtProducer
    database = DatabaseEnum.JCEtalon