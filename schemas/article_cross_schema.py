from pydantic import BaseModel

class ArticleCrossSchema(BaseModel):
    manufacturerId: int
    OENbr: str
    SupplierId: int
    PartsDataSupplierArticleNumber: str

    class Config:
        from_attributes = True