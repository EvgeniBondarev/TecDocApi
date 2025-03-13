from pydantic import BaseModel
from typing import Optional


class ArticleAttributesSchema(BaseModel):
    supplierid: int
    datasupplierarticlenumber: str
    id: int
    description: Optional[str]
    displaytitle: str
    displayvalue: str

    class Config:
        from_attributes = True
