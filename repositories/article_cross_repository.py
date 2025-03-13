from db.database_enum import DatabaseEnum
from models.TD2018.article_cross import ArticleCross
from repositories.ABC.sql_alchemy_read_repository import SQLAlchemyReadRepository


class ArticleCrossRepository(SQLAlchemyReadRepository):
    model = ArticleCross
    database = DatabaseEnum.TD2018
