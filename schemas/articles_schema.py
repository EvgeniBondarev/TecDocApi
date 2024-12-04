from pydantic import BaseModel
from typing import Optional

class ArticlesSchema(BaseModel):
    supplierId: int
    DataSupplierArticleNumber: str
    ArticleStateDisplayValue: str
    Description: str
    FlagAccessory: str
    FlagMaterialCertification: str
    FlagRemanufactured: str
    FlagSelfServicePacking: str
    FoundString: str
    HasAxle: str
    HasCommercialVehicle: str
    HasCVManuID: str
    HasEngine: str
    HasLinkitems: str
    HasMotorbike: str
    HasPassengerCar: str
    IsValid: str
    LotSize1: Optional[int] = None
    LotSize2: Optional[int] = None
    NormalizedDescription: str
    PackingUnit: Optional[int] = None
    QuantityPerPackingUnit: Optional[int] = None

    class Config:
        from_attributes = True
