from pydantic import BaseModel
from typing import Optional

class ProductInformationSchema(BaseModel):
    TOW_KOD: str
    IC_INDEX: Optional[str]
    TEC_DOC: Optional[str]
    TEC_DOC_PROD: Optional[int]
    ARTICLE_NUMBER: Optional[str]
    MANUFACTURER: Optional[str]
    SHORT_DESCRIPTION: Optional[str]
    DESCRIPTION: Optional[str]
    BARCODES: Optional[str]
    PACKAGE_WEIGHT: Optional[str]
    PACKAGE_LENGTH: Optional[str]
    PACKAGE_WIDTH: Optional[str]
    PACKAGE_HEIGHT: Optional[str]
    CUSTOM_CODE: Optional[str]

    class Config:
        from_attributes = True