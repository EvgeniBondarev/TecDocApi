from pydantic import BaseModel, Field, validator
from typing import Optional, Union
import json

class BatchSearchResult(BaseModel):
    brand: str
    number: str
    numberFix: str
    description: str
    availability: int
    packing: int
    deliveryPeriod: int
    deliveryPeriodMax: Optional[int] = Field(None)
    distributorCode: str
    supplierCode: int
    supplierColor: Optional[Union[str, int]] = Field(None)
    supplierDescription: str
    itemKey: str
    price: float
    maxPrice: float
    weight: float
    volume: Optional[float] = None
    deliveryProbability: int
    descriptionOfDeliveryProbability: str
    additionalPrice: float
    noReturn: bool
    grp: Optional[str] = None
    code: Optional[str] = None
    priceIn: Optional[float] = None
    priceRate: Optional[float] = None

    class Config:
        from_attributes = True

    # Валидаторы для обработки нестандартных значений
    @validator('deliveryPeriodMax', pre=True)
    def empty_str_to_none(cls, v):
        if v == "":
            return None
        return v

    @validator('supplierColor', pre=True)
    def convert_supplier_color(cls, v):
        if v is None:
            return None
        return str(v)  # Конвертируем в строку, если приходит число

    @validator('volume', 'priceIn', 'priceRate', pre=True)
    def empty_str_to_none_float(cls, v):
        if v == "":
            return None
        return v