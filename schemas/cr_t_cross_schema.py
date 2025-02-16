from pydantic import BaseModel, field_validator
from typing import Optional

class CrTCrossSchema(BaseModel):
    cr_id: Optional[str]
    cr_cross: Optional[str]
    cr_crosscode: Optional[str]
    cr_maincode: Optional[str]
    cr_by: Optional[str]
    cr_bycode: Optional[str]
    cr_verity: Optional[str]
    cr_session_id: Optional[str]
    cr_deleted: Optional[str]
    cr_ismainnew: Optional[str]
    cr_date: Optional[str]

    @field_validator("cr_crosscode", "cr_maincode", "cr_bycode", mode="before")
    @classmethod
    def strip_whitespace(cls, value: Optional[str]) -> Optional[str]:
        return value.strip() if isinstance(value, str) else value

    class Config:
        from_attributes = True
