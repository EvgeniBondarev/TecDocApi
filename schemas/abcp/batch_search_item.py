from pydantic import BaseModel, validator

class BatchSearchItem(BaseModel):
    brand: str
    number: str