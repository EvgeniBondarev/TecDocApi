from sqlalchemy import Column, Integer, String, SmallInteger, Index

from models.base_model import Base


class ArticleCross(Base):
    __tablename__ = "article_cross"

    manufacturerId = Column(Integer, primary_key=True, nullable=False)
    OENbr = Column(String, primary_key=True, nullable=False)
    SupplierId = Column(SmallInteger, primary_key=True, nullable=False)
    PartsDataSupplierArticleNumber = Column(String, primary_key=True, nullable=False)

    __table_args__ = (
        Index("manufacturerId", "manufacturerId"),
        Index("OENbr", "OENbr"),
        Index("SupplierId_PartsDataSupplierArticleNumber", "SupplierId", "PartsDataSupplierArticleNumber"),
        {'mysql_charset': 'utf8', 'mysql_collate': 'utf8_general_ci', 'mysql_row_format': 'COMPRESSED'}
    )