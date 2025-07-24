from sqlalchemy import Column, String, Integer
from models.base_model import Base

class ProductInformation(Base):
    __tablename__ = 'ProductInformation'

    TOW_KOD = Column(String(10), primary_key=True)
    IC_INDEX = Column(String(255), nullable=True)
    TEC_DOC = Column(String(255), nullable=True)
    TEC_DOC_PROD = Column(Integer, nullable=True)
    ARTICLE_NUMBER = Column(String(255), nullable=True)
    MANUFACTURER = Column(String(255), nullable=True)
    SHORT_DESCRIPTION = Column(String(255), nullable=True)
    DESCRIPTION = Column(String(255), nullable=True)
    BARCODES = Column(String(255), nullable=True)
    PACKAGE_WEIGHT = Column(String(255), nullable=True)
    PACKAGE_LENGTH = Column(String(255), nullable=True)
    PACKAGE_WIDTH = Column(String(255), nullable=True)
    PACKAGE_HEIGHT = Column(String(255), nullable=True)
    CUSTOM_CODE = Column(String(255), nullable=True)