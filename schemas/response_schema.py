from pydantic import BaseModel
from typing import Generic, TypeVar, List

T = TypeVar("T")

class ResponseSchema(BaseModel, Generic[T]):
    error: bool
    message: str
    result: List[T]
