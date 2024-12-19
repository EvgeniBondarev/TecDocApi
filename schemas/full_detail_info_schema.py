from typing import List, Optional

from pydantic import BaseModel

from schemas.article_attributes_schema import ArticleAttributesSchema
from schemas.article_ean_schema import ArticleEANSchema
from schemas.articles_schema import ArticlesSchema
from schemas.et_producer_schema import EtProducerSchema
from schemas.suppliers_schema import SuppliersSchema


class FullDetailInfoSchema(BaseModel):
    normalized_article: str
    article_ean : Optional[ArticleEANSchema]
    article_schema : Optional[ArticlesSchema]
    detail_attribute: List[ArticleAttributesSchema]
    img_urls: List[str]
    supplier_from_jc: Optional[EtProducerSchema]
    supplier_from_td: Optional[SuppliersSchema]