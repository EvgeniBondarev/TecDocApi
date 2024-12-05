from pydantic import BaseModel


class ArticleLinksSchema(BaseModel):
    supplierid: int
    productid: int
    linkagetypeid: int
    linkageid: int
    datasupplierarticlenumber: str


    class Config:
        from_attributes = True
