from sqlalchemy import Column, SmallInteger, String, Index

from models.base_model import Base


class ArticleAttributes(Base):
    __tablename__ = "article_attributes"

    supplierid = Column(SmallInteger(), primary_key=True)

    datasupplierarticlenumber = Column(String(32), primary_key=True)
    id = Column(SmallInteger(), primary_key=True)
    description = Column(String(128), nullable=True)
    displaytitle = Column(String(128), nullable=False)
    displayvalue = Column(String(4000), nullable=False)

    # Индексы
    __table_args__ = (
        Index("article", "supplierid", "datasupplierarticlenumber"),
        Index("id", "id"),
    )
