from typing import Optional, List

from pydantic import BaseModel

from schemas.volva.applicability.vehicle_specification import VehicleSpecification


class VehicleModification(BaseModel):
    name: str
    power: Optional[str]  # Мощность, Л.С.
    engine_type: Optional[str]  # Тип / Код двигателя
    engine_volume: Optional[str]  # Объем двигателя
    drive_type: Optional[str]  # Привод
    production_period: Optional[str]  # Период выпуска
    specifications: List[VehicleSpecification]  # Дополнительные характеристики