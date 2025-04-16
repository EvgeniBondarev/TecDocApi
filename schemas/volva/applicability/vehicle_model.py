from typing import List

from pydantic import BaseModel

from schemas.volva.applicability.vehicle_modification import VehicleModification


class VehicleModel(BaseModel):
    name: str
    modifications: List[VehicleModification]