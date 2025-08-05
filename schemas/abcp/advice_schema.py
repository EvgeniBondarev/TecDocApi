from pydantic import BaseModel

class AdviceSchema(BaseModel):
    brand: str
    number: str
    total: int

    class Config:
        from_attributes = True
