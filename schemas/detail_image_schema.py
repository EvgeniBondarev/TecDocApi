from typing import List

from pydantic import BaseModel

class DetailImageSchema(BaseModel):
    ImageUrls: List[str]