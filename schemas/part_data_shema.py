from typing import List, Dict

from pydantic import BaseModel, HttpUrl

from schemas.replacement_part_schema import ReplacementPartSchema


class PartDataSchema(BaseModel):
    name: str
    images: List[HttpUrl]
    specifications: Dict[str, str]
    replacements: List[ReplacementPartSchema]