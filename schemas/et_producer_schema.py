from pydantic import BaseModel
from typing import Optional

class EtProducerSchema(BaseModel):
    id: int
    realid: Optional[int]
    prefix: Optional[str]
    name: Optional[str]
    address: Optional[str]
    www: Optional[str]
    rating: Optional[int]
    existName: Optional[str]
    existId: Optional[int]
    domain: Optional[str]
    tecdocSupplierId: Optional[int]
    marketPrefix: Optional[str]
    img: Optional[str] = None

    class Config:
        from_attributes = True
