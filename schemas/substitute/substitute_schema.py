from typing import List

from pydantic import BaseModel

from schemas.substitute.attribute_schema import Attribute
from schemas.substitute.modification_schema import Modification

class SubstituteSchema(BaseModel):
    Type: str
    Name: str
    ModelId: int
    Modification: Modification
    Attributes: List[Attribute]