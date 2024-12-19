from ast import Index
from tokenize import String

from sqlalchemy import SmallInteger, Column

from models.base_model import Base


from sqlalchemy import Index, Column, SmallInteger, String
from sqlalchemy.ext.declarative import declarative_base

Base = declarative_base()

class ArticleEAN(Base):
    __tablename__ = "article_ean"

    supplierid = Column(SmallInteger(), primary_key=True)
    datasupplierarticlenumber = Column(String, primary_key=True)
    ean = Column(String, nullable=False)

    # Индексы с уникальными именами
    __table_args__ = (
        Index("ix_supplierid_datasupplierarticlenumber", "supplierid", "datasupplierarticlenumber"),
        Index("ix_ean", "ean"),
    )
