from sqlalchemy import Column, SmallInteger, String, Integer, Index

from models.base_model import Base


class ArticleLi(Base):
    __tablename__ = "article_li"

    supplierId = Column(SmallInteger(), primary_key=True)
    DataSupplierArticleNumber = Column(String(32), primary_key=True)
    linkageTypeId = Column(String(32), nullable=False)
    linkageId = Column(Integer(), nullable=False)

    __table_args__ = (
        Index("supplierId", "supplierId", "DataSupplierArticleNumber"),
        Index("linkageTypeId", "linkageTypeId"),
    )