from typing import List, Optional

from pydantic import BaseModel

from schemas.pr_attribute_schema import PrAttributeSchema


class PrProductResponseSchema(BaseModel):
    article: str
    brand: str
    Vendor_Code: str
    images: List[str]
    attributes: List[PrAttributeSchema]
    Vendor_Category_Name: Optional[str]
    OEM_Code: Optional[str]
    OEM_Mark: Optional[str]
    models: List[str]

    class Config:
        from_attributes = True