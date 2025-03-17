from pydantic import BaseModel
from typing import Literal, Optional

class ManufacturerSchema(BaseModel):
    id: int
    canbedisplayed: Optional[Literal['True', 'False']] = None
    description: Optional[str] = None
    fulldescription: Optional[str] = None
    haslink: Optional[Literal['True', 'False']] = None
    isaxle: Literal['True', 'False']
    iscommercialvehicle: Literal['True', 'False']
    isengine: Literal['True', 'False']
    ismotorbike: Literal['True', 'False']
    ispassengercar: Literal['True', 'False']
    istransporter: Literal['True', 'False']
    isvgl: Optional[Literal['True', 'False']] = None
    matchcode: Optional[str] = None

    class Config:
        from_attributes = True  # Для совместимости с ORM