from pydantic import BaseModel
from typing import Optional

class EtProducerSchema(BaseModel):
    id: int
    realid: Optional[str]
    prefix: Optional[str]
    name: Optional[str]
    address: Optional[str]
    www: Optional[str]
    rating: Optional[str]
    existName: Optional[str]
    existId: Optional[str]
    domain: Optional[str]
    tecdocSupplierId: Optional[str]
    marketPrefix: Optional[str]

    class Config:
        from_attributes = True
