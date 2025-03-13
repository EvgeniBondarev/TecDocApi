from db.database_enum import DatabaseEnum
from models.TD2018.article_ean import ArticleEAN
from repositories.ABC.sql_alchemy_read_repository import SQLAlchemyReadRepository


class ArticleEANRepository(SQLAlchemyReadRepository):
    model = ArticleEAN
    database = DatabaseEnum.TD2018
