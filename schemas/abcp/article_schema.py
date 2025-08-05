from pydantic import BaseModel, validator
from typing import Optional

class ArticleSchema(BaseModel):
    distributorId: int
    grp: Optional[str] = None
    code: str
    brand: str
    number: str
    numberFix: str
    description: str
    availability: int
    packing: int
    deliveryPeriod: int
    deliveryPeriodMax: int
    deadlineReplace: str
    distributorCode: str
    supplierCode: int
    supplierColor: Optional[str] = None
    supplierDescription: str
    itemKey: str
    price: float
    maxPrice: float
    weight: float
    volume: Optional[float] = None
    lastUpdateTime: str
    additionalPrice: float
    noReturn: bool
    isUsed: bool
    meta: dict
    deliveryProbability: int
    descriptionOfDeliveryProbability: str
    priceIn: float
    priceRate: float
    isAnalog: bool

    class Config:
        from_attributes = True

    # Валидаторы для замены пустых строк на 0
    @validator('deliveryPeriodMax', 'availability', 'packing', 'deliveryPeriod',
               'supplierCode', 'deliveryProbability', pre=True)
    def empty_str_to_zero(cls, v):
        if isinstance(v, str) and v.strip() == "":
            return 0
        return v

    @validator('price', 'maxPrice', 'weight', 'additionalPrice',
              'priceIn', 'priceRate', pre=True)
    def empty_str_to_float_zero(cls, v):
        if isinstance(v, str) and v.strip() == "":
            return 0.0
        return v