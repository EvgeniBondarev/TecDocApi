from typing import List

from pydantic import BaseModel

from schemas.substitute.model_substitute_schema import ModelSubstituteSchema
from schemas.substitute.substitute_schema import SubstituteSchema


class SubstituteResultSchema(BaseModel):
    Models: List[ModelSubstituteSchema]
    SubstitutesCount: int

