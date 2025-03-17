from pydantic import BaseModel, field_validator
from typing import Optional
from datetime import datetime

from schemas.et_producer_schema import EtProducerSchema


class CrTCrossSchema(BaseModel):
    cr_id: Optional[int] = None
    cr_cross: int
    cr_crosscode: str
    cr_maincode: Optional[str] = None
    cr_by: int
    cr_bycode: str
    cr_verity: int = 0
    cr_session_id: int
    cr_deleted: Optional[bool] = False
    cr_ismainnew: bool = False
    cr_date: Optional[datetime] = None
    et_producer: Optional[EtProducerSchema] = None

    @field_validator("cr_crosscode", "cr_maincode", "cr_bycode", mode="before")
    @classmethod
    def strip_whitespace(cls, value: Optional[str]) -> Optional[str]:
        return value.strip() if isinstance(value, str) else value

    class Config:
        from_attributes = True  # Для совместимости с ORM