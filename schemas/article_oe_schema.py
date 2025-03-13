from pydantic import BaseModel
from typing import Literal, Optional

from schemas.suppliers_schema import SuppliersSchema


class ArticleOESchema(BaseModel):
    supplierid: int
    datasupplierarticlenumber: str
    IsAdditive: Literal['True', 'False']
    OENbr: str
    manufacturerId: int
    supplier: Optional[SuppliersSchema] = None

    class Config:
        from_attributes = True  