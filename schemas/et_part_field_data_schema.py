from pydantic import BaseModel
from typing import Optional


class EtPartFieldDataSchema(BaseModel):
    id: int
    data: Optional[str]

    class Config:
        from_attributes = True
