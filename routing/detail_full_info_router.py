from http.client import HTTPException
from typing import List

from fastapi import APIRouter, Depends

from S3.s3_service import S3Service
from dependencies import get_article_images_service, get_article_attributes_service, get_et_producer_service, \
    get_articles_service, get_suppliers_service, get_s3_service
from schemas import ArticleAttributesSchema
from schemas.articles_schema import ArticlesSchema
from schemas.aticle_image_schema import ArticleImageSchema
from schemas.et_producer_schema import EtProducerSchema
from services.article_attributes_service import ArticleAttributesService
from services.article_images_service import ArticleImagesService
from services.articles_service import ArticlesService
from services.et_producer_service import EtProducerService
from services.suppliers_service import SuppliersService

router = APIRouter(
    prefix="/detail_full_info",
    tags=["detail_full_info"]
)

@router.get("/{supplier}/{article}", response_model=dict)
async def get_full_detail_info(
    supplier: str,
    article: str,
    article_attributes_service : ArticleAttributesService =Depends(get_article_attributes_service),
    et_producer_service : EtProducerService =Depends(get_et_producer_service),
    article_images_service: ArticleImagesService =Depends(get_article_images_service),
    articles_service : ArticlesService = Depends(get_articles_service),
    suppliers_service : SuppliersService = Depends(get_suppliers_service),
    s3_service : S3Service = Depends(get_s3_service)
    ):

    data_supplier_article_number : ArticlesSchema = await articles_service.get_articles_by_found_string(article)
    if data_supplier_article_number is not None:
        article = data_supplier_article_number.DataSupplierArticleNumber

    supplier_from_jc : EtProducerSchema = await et_producer_service.get_producers_by_name(supplier)

    if int(supplier_from_jc.id) != int(supplier_from_jc.realid):
        supplier_from_jc = await et_producer_service.get_producer_by_id(int(supplier_from_jc.realid))

    if int(supplier_from_jc.tecdocSupplierId) == 0:
        supplier_from_td = await suppliers_service.get_suppliers_by_description_case_ignore(supplier)

        if supplier_from_jc.prefix != None and supplier_from_jc.prefix != "" and supplier_from_td is None:
            supplier_from_td = await suppliers_service.get_suppliers_by_matchcode(supplier_from_jc.prefix)
            if supplier_from_td is None:
                supplier_from_td = await suppliers_service.get_suppliers_by_matchcode(supplier_from_jc.marketPrefix)

        supplier_id = int(supplier_from_td.id)
    else:
        supplier_id = supplier_from_jc.id

    detail_attribute : List[ArticleAttributesSchema] = await article_attributes_service.get_attribute_by_filter(supplier_id,
                                                                                                                article)

    images : List[ArticleImageSchema] = await article_images_service.get_images_by_filter(supplier_id, article)

    img_urls = []
    for image in images:
        url = s3_service.get_image_url(image.PictureName)
        if url is not None:
            img_urls.append(url)

    return {
        "detail_attribute": detail_attribute,
        "img_urls": img_urls,
        "supplier_from_jc": supplier_from_jc,
        "supplier_from_td": supplier_from_td,
    }