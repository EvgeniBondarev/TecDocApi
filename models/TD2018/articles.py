from sqlalchemy import (
    Column, String, SmallInteger, Integer, Enum, Index
)

from models.base_model import Base


class Articles(Base):
    __tablename__ = "articles"

    supplierId = Column(SmallInteger(), primary_key=True, nullable=False)
    DataSupplierArticleNumber = Column(String(32), primary_key=True, nullable=False)

    ArticleStateDisplayValue = Column(String(128), nullable=False)
    Description = Column(String(128), nullable=False)
    FlagAccessory = Column(Enum("True", "False"), nullable=False)
    FlagMaterialCertification = Column(Enum("True", "False"), nullable=False)
    FlagRemanufactured = Column(Enum("True", "False"), nullable=False)
    FlagSelfServicePacking = Column(Enum("True", "False"), nullable=False)
    FoundString = Column(String(64), nullable=False)
    HasAxle = Column(Enum("True", "False"), nullable=False)
    HasCommercialVehicle = Column(Enum("True", "False"), nullable=False)
    HasCVManuID = Column(Enum("True", "False"), nullable=False)
    HasEngine = Column(Enum("True", "False"), nullable=False)
    HasLinkitems = Column(Enum("True", "False"), nullable=False)
    HasMotorbike = Column(Enum("True", "False"), nullable=False)
    HasPassengerCar = Column(Enum("True", "False"), nullable=False)
    IsValid = Column(Enum("True", "False"), nullable=False)
    LotSize1 = Column(Integer(), nullable=True)
    LotSize2 = Column(Integer(), nullable=True)
    NormalizedDescription = Column(String(128), nullable=False)
    PackingUnit = Column(Integer(), nullable=True)
    QuantityPerPackingUnit = Column(Integer(), nullable=True)

    __table_args__ = (
        Index("idx_supplierId", "supplierId"),
        Index("idx_DataSupplierArticleNumber", "DataSupplierArticleNumber"),
        Index("idx_FoundString", "FoundString"),
    )
