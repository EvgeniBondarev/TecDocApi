from db.database_enum import DatabaseEnum
from models.TD2018.article_attributes import ArticleAttributes
from repositories.ABC.sql_alchemy_read_repository import SQLAlchemyReadRepository


class ArticlesAttributesRepository(SQLAlchemyReadRepository):
    model = ArticleAttributes
    database = DatabaseEnum.TD2018