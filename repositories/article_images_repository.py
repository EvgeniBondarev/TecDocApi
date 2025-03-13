from db.database_enum import DatabaseEnum
from models.TD2018.article_images import ArticleImages
from repositories.ABC.sql_alchemy_read_repository import SQLAlchemyReadRepository


class ArticlesImagesRepository(SQLAlchemyReadRepository):
    model = ArticleImages
    database = DatabaseEnum.TD2018