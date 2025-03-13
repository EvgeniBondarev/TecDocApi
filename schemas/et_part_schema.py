from typing import Optional

from pydantic import validator, BaseModel


class EtPartSchema(BaseModel):
    id: int
    producerId: int
    oldId: int
    code: str
    longcode: str
    weight: float
    name: Optional[str]
    description: Optional[str]
    V: float
    sessionid: int
    nochangeflag: bool
    accepted: bool
    deleted: bool
    rating: int
    old: bool
    dead: bool

    class Config:
        from_attributes = True

    @validator('name', pre=True, always=True)
    def convert_name_to_str(cls, v):
        if v is not None:
            return str(v)
        return v

    @validator('description', pre=True, always=True)
    def convert_description_to_str(cls, v):
        if v is not None:
            return str(v)
        return v