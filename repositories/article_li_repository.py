from db.database_enum import DatabaseEnum
from models.TD2018.article_li import ArticleLi
from repositories.ABC.sql_alchemy_read_repository import SQLAlchemyReadRepository


class ArticleLiRepository(SQLAlchemyReadRepository):
    model = ArticleLi
    database = DatabaseEnum.TD2018