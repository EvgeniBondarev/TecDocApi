from pydantic import BaseModel

class ArticleEANSchema(BaseModel):
    supplierid: int
    datasupplierarticlenumber: str
    ean: str

    class Config:
        from_attributes = True
