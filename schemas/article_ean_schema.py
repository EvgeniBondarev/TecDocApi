from pydantic import BaseModel

from schemas.suppliers_schema import SuppliersSchema


class ArticleEANSchema(BaseModel):
    supplierid: int
    datasupplierarticlenumber: str
    ean: str
    supplier: SuppliersSchema = None

    class Config:
        from_attributes = True
