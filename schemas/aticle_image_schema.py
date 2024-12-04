from pydantic import BaseModel


class ArticleImageSchema(BaseModel):
    supplierId: int
    DataSupplierArticleNumber: str
    AdditionalDescription: str
    Description: str
    DocumentName: str
    DocumentType: str
    NormedDescriptionID: int
    PictureName: str
    ShowImmediately: str

    class Config:
        from_attributes = True
