from pydantic import BaseModel, field_validator
from typing import Optional, Any


class SupplierDetailsSchema(BaseModel):
    supplierid: int
    addresstypeid: str

    addresstype: Optional[str] = None
    city1: Optional[str] = None
    city2: Optional[str] = None
    countrycode: Optional[str] = None
    email: Optional[str] = None
    fax: Optional[str] = None
    homepage: Optional[str] = None
    name1: Optional[str] = None
    name2: Optional[str] = None
    postalcodecity: Optional[str] = None
    postalcodepob: Optional[str] = None
    postalcodewholesaler: Optional[str] = None
    postalcountrycode: Optional[str] = None
    postofficebox: Optional[str] = None
    street1: Optional[str] = None
    street2: Optional[str] = None
    telephone: Optional[str] = None

    @field_validator('*', mode='before')
    def empty_str_to_none(cls, v: Any, info):
        # Получаем имя текущего поля из info
        field_name = info.field_name
        if field_name in {'supplierid', 'addresstypeid'}:
            return v
        if isinstance(v, str) and v.strip() == "":
            return None
        return v

    class Config:
        from_attributes = True