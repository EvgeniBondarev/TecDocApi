from http.client import HTTPException
from typing import List

from fastapi import APIRouter, Depends

from dependencies import get_article_images_service, get_article_attributes_service, get_et_producer_service
from schemas import ArticleAttributesSchema
from schemas.aticle_image_schema import ArticleImageSchema
from schemas.et_producer_schema import EtProducerSchema
from services.article_attributes_service import ArticleAttributesService
from services.article_images_service import ArticleImagesService
from services.et_producer_service import EtProducerService

router = APIRouter(
    prefix="/detail_full_info",
    tags=["detail_full_info"]
)

@router.get("/{supplier}/{article}", response_model=str)
async def get_full_detail_info(
    supplier: str,
    article: str,
    article_attributes_service : ArticleAttributesService =Depends(get_article_attributes_service),
    et_producer_service : EtProducerService =Depends(get_et_producer_service),
    article_images_service: ArticleImagesService =Depends(get_article_images_service)):

    supplier : EtProducerSchema = await et_producer_service.get_producers_by_name(supplier)

    if not supplier:
        raise HTTPException(status_code=404, detail="Supplier not found")

    if supplier.id != supplier.realid:
        supplier = await et_producer_service.get_producer_by_id(supplier.realid)

    if supplier.tecdocSupplierId is None:
        raise HTTPException(status_code=404, detail="TecdocSupplierId not found")

    #TODO: Сделать поска артикула через таблицу артикулов (поле FoundString)

    detail_attribute : List[ArticleAttributesSchema] = await article_attributes_service.get_attribute_by_filter(supplier.tecdocSupplierId,
                                                                                                                article)

    if detail_attribute is None:
        raise HTTPException(status_code=404, detail="Detail Attribute not found")



    images : List[ArticleImageSchema] = await article_images_service.get_images_by_filter(supplier.tecdocSupplierId, article)

    print("------------------------------")
    print(str(supplier))
    print(str(detail_attribute))
    print(str(images))
    print("------------------------------")


    return str(images)