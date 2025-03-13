from typing import List

from pydantic import BaseModel

from schemas.substitute.substitute_schema import SubstituteSchema


class SubstituteResultSchema(BaseModel):
    Substitutes : List[SubstituteSchema]
    SubstitutesCount : int

