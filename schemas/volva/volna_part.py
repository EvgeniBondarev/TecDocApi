from typing import List, Optional

from pydantic import BaseModel

from schemas.substitute.attribute_schema import Attribute
from schemas.volva.applicability.vehicle_manufacturer import VehicleManufacturer
from schemas.volva.manufacturer_cross_numbers import ManufacturerCrossNumbers


class VolnaPart(BaseModel):
    name: str
    images: List[str]
    attributes: List[Attribute]
    characteristics: List[Attribute]
    manufacturer_image: Optional[str]
    cross_numbers: List[ManufacturerCrossNumbers]