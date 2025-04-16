from typing import Optional, List

from pydantic import BaseModel

from schemas.volva.applicability.vehicle_model import VehicleModel


class VehicleManufacturer(BaseModel):
    name: str
    logo_url: Optional[str]
    models: List[VehicleModel]
