from pydantic import BaseModel


class VehicleSpecification(BaseModel):
    name: str
    value: str