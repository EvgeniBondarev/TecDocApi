from typing import Optional

from pydantic import BaseModel


class CrossNumberItem(BaseModel):
    number: str
    search_link: Optional[str]