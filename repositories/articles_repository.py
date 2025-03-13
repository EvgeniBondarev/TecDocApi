from db.database_enum import DatabaseEnum
from models.TD2018.articles import Articles
from repositories.ABC.sql_alchemy_read_repository import SQLAlchemyReadRepository


class ArticlesRepository(SQLAlchemyReadRepository):
    model = Articles
    database = DatabaseEnum.TD2018