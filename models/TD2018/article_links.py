from sqlalchemy import Column, Integer, String, Index
from sqlalchemy.ext.declarative import declarative_base

Base = declarative_base()

class ArticleLinks(Base):
    __tablename__ = "article_links"

    supplierid = Column(Integer, primary_key=True, nullable=False, autoincrement=False)
    productid = Column(Integer, primary_key=True, nullable=False, autoincrement=False)
    linkagetypeid = Column(Integer, primary_key=True, nullable=False, autoincrement=False)
    linkageid = Column(Integer, primary_key=True, nullable=False, autoincrement=False)
    datasupplierarticlenumber = Column(String(32), primary_key=True, nullable=False)

    __table_args__ = (
        Index("idx_productid", "productid"),
        Index("idx_linkagetypeid", "linkagetypeid"),
        Index("idx_linkageid", "linkageid"),
        Index("idx_supplierid", "supplierid"),
        Index("idx_datasupplierarticlenumber", "datasupplierarticlenumber"),
    )
