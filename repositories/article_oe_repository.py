from db.database_enum import DatabaseEnum
from models.TD2018.article_oe import ArticleOE
from repositories.ABC.sql_alchemy_read_repository import SQLAlchemyReadRepository


class ArticleOERepository(SQLAlchemyReadRepository):
    model = ArticleOE
    database = DatabaseEnum.TD2018