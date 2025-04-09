from pydantic import BaseModel
from typing import Literal, Optional

from schemas.manufacturer_schema import ManufacturerSchema
from schemas.suppliers_schema import SuppliersSchema


class ArticleOESchema(BaseModel):
    supplierid: int
    datasupplierarticlenumber: str
    IsAdditive: bool
    OENbr: str
    manufacturerId: int
    supplier: Optional[SuppliersSchema] = None
    manufacturer: Optional[ManufacturerSchema] = None

    class Config:
        from_attributes = True  