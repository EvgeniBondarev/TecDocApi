from pydantic import BaseModel
from typing import Optional


class EtStringSchema(BaseModel):
    id: int
    producerId: Optional[int]
    oldId: Optional[int]
    idstr: Optional[int]
    lng: Optional[int]
    text: Optional[str]

    class Config:
        from_attributes = True
