from pydantic import BaseModel

from schemas.suppliers_schema import  SuppliersSchema
from schemas.et_producer_schema import EtProducerSchema
from schemas.detail_image_schema import DetailImageSchema
from schemas.article_attributes_schema import ArticleAttributesSchema

class DetailFullInfoSchema(BaseModel):
    SuppliersSchema: SuppliersSchema
    EtProducerSchema: EtProducerSchema
    DetailImageSchema: DetailImageSchema
    ArticleAttributesSchema: ArticleAttributesSchema
