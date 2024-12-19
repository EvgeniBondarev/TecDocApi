from pydantic import BaseModel


class Attribute(BaseModel):
    Title: str
    Value: str