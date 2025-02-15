from pydantic import BaseModel


class PrAttributeSchema(BaseModel):
    name: str
    value: str