from typing import List, Optional

from pydantic import BaseModel

from schemas.et_producer_schema import EtProducerSchema
from schemas.suppliers_schema import SuppliersSchema


class AllSuppliersSchema(BaseModel):
    suppliersFromTd: Optional[List[SuppliersSchema]]
    suppliersFromJs:  Optional[List[EtProducerSchema]]