from pydantic import BaseModel
from typing import Optional


class ArticleAttributesSchema(BaseModel):
    id: int
    supplierid: int
    datasupplierarticlenumber: str
    description: Optional[str]
    displaytitle: str
    displayvalue: str

    class Config:
        from_attributes = True
