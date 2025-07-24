from db.database_enum import DatabaseEnum
from models.MNK.product_information import ProductInformation
from repositories.ABC.sql_alchemy_read_repository import SQLAlchemyReadRepository


class ProductInformationRepository(SQLAlchemyReadRepository):
    model = ProductInformation
    database = DatabaseEnum.MNK # Замените на вашу базу данных