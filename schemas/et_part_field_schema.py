from pydantic import BaseModel
from typing import Optional


class EtPartFieldSchema(BaseModel):
    id: Optional[str]
    fieldid: Optional[str]
    producerid: Optional[str]
    dataid: Optional[str]

    class Config:
        from_attributes = True
