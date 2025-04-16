from typing import List

from pydantic import BaseModel

from schemas.volva.cross_numberItem import CrossNumberItem


class ManufacturerCrossNumbers(BaseModel):
    manufacturer: str
    numbers: List[CrossNumberItem]
