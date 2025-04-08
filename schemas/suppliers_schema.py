from pydantic import BaseModel
from typing import Optional

class SuppliersSchema(BaseModel):
    id: int
    dataversion: Optional[int] = None
    description: Optional[str] = None
    matchcode: Optional[str] = None
    nbrofarticles: Optional[int] = None
    hasnewversionarticles: Optional[bool] = None
    img: Optional[str] = None

    class Config:
        from_attributes = True