from pydantic import BaseModel


class Modification(BaseModel):
    description: str
    construction_interval: str