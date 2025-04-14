from typing import List

from pydantic import BaseModel

from schemas.substitute.substitute_schema import SubstituteSchema


class ModelSubstituteSchema(BaseModel):
    ModelName: str
    ModelId: int
    Substitutes:  List[SubstituteSchema]