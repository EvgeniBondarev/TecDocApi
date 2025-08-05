from pydantic import BaseModel

class TipSchema(BaseModel):
    brand: str
    number: str
    description: str

    class Config:
        from_attributes = True