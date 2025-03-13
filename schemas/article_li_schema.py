from pydantic import BaseModel

class ArticleLiSchema(BaseModel):
    supplierId: int
    DataSupplierArticleNumber: str
    linkageTypeId: str
    linkageId: int

    class Config:
        from_attributes = True
