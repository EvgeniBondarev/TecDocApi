from typing import Optional

from pydantic import BaseModel, HttpUrl


class ReplacementPartSchema(BaseModel):
    brand: str
    article: str
    image: Optional[HttpUrl]
    price: Optional[float]
    quantity: Optional[int]
    delivery: Optional[str]