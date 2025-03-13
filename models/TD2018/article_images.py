from sqlalchemy import Column, SmallInteger, String, Enum, Index

from models.base_model import Base


class ArticleImages(Base):
    __tablename__ = "article_images"

    supplierId = Column(SmallInteger(), primary_key=True)
    DataSupplierArticleNumber = Column(String(32), primary_key=True)

    AdditionalDescription = Column(String(64), nullable=False)
    Description = Column(String(64), nullable=False)
    DocumentName = Column(String(128), nullable=False)
    DocumentType = Column(String(8), nullable=False)
    NormedDescriptionID = Column(SmallInteger(), nullable=False)
    PictureName = Column(String(64), nullable=False)
    ShowImmediately = Column(Enum('True', 'False'), nullable=False)

    __table_args__ = (
        Index("supplierId", "supplierId", "DataSupplierArticleNumber"),
        Index("DocumentType", "DocumentType"),
    )
