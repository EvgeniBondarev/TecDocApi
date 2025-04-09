from pydantic import BaseModel
from typing import Literal, Optional

from pydantic import BaseModel
from typing import Optional

class ManufacturerSchema(BaseModel):
    id: int
    canbedisplayed: Optional[bool] = None
    description: Optional[str] = None
    fulldescription: Optional[str] = None
    haslink: Optional[bool] = None
    isaxle: bool
    iscommercialvehicle: bool
    isengine: bool
    ismotorbike: bool
    ispassengercar: bool
    istransporter: bool
    isvgl: Optional[bool] = None
    matchcode: Optional[str] = None

    class Config:
        from_attributes = True
