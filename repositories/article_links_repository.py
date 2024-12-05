from db.database_enum import DatabaseEnum
from models.TD2018.article_links import ArticleLinks
from repositories.ABC.sql_alchemy_read_repository import SQLAlchemyReadRepository


class ArticleLinksRepository(SQLAlchemyReadRepository):
    model = ArticleLinks
    database = DatabaseEnum.TD2018