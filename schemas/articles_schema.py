from pydantic import BaseModel, field_validator
from typing import Optional

class ArticlesSchema(BaseModel):
    supplierId: int
    DataSupplierArticleNumber: str
    ArticleStateDisplayValue: str
    Description: str
    FlagAccessory: bool
    FlagMaterialCertification: bool
    FlagRemanufactured: bool
    FlagSelfServicePacking: bool
    FoundString: str
    HasAxle: bool
    HasCommercialVehicle: bool
    HasCVManuID: bool
    HasEngine: bool
    HasLinkitems: bool
    HasMotorbike: bool
    HasPassengerCar: bool
    IsValid: bool
    LotSize1: Optional[int] = None
    LotSize2: Optional[int] = None
    NormalizedDescription: str
    PackingUnit: Optional[int] = None
    QuantityPerPackingUnit: Optional[int] = None

    @field_validator("FoundString",  mode="before")
    @classmethod
    def strip_spaces(cls, v: str) -> str:
        return v.strip() if isinstance(v, str) else v

    class Config:
        from_attributes = True
