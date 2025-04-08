import asyncio
from typing import List

from fastapi import APIRouter, Depends, HTTPException

from S3.s3_service import S3Service
from dependencies import get_articles_service, get_suppliers_service, get_et_part_service, get_et_producer_service, \
    get_article_images_service, get_s3_service
from schemas.all_suppliers_schema import AllSuppliersSchema
from schemas.et_producer_schema import EtProducerSchema
from schemas.suppliers_schema import SuppliersSchema
from services.article_images_service import ArticleImagesService
from services.articles_service import ArticlesService
from services.et_part_service import EtPartService
from services.et_producer_service import EtProducerService
from services.suppliers_service import SuppliersService
from services.utils.search_preparation import get_normalized_article, fetch_image_urls, get_supplier_id

router = APIRouter(
    prefix="/suppliers",
    tags=["Supplier"]
)

async def attach_image(entity, name_field: str, normalized_article: str,
                       et_producer_service: EtProducerService,
                       suppliers_service: SuppliersService,
                       article_images_service: ArticleImagesService,
                       s3_service: S3Service):
    name = getattr(entity, name_field)
    supplier_id = await get_supplier_id(name, et_producer_service, suppliers_service)
    images = await article_images_service.get_images_by_filter(supplier_id, normalized_article)
    img_urls = await fetch_image_urls(images, s3_service)
    if img_urls:
        entity.img = img_urls[0]


@router.get("/{article}", response_model=AllSuppliersSchema)
async def get_suppliers_by_article(
    article: str,
    articles_service: ArticlesService = Depends(get_articles_service),
    suppliers_service: SuppliersService = Depends(get_suppliers_service),
    et_part_service: EtPartService = Depends(get_et_part_service),
    et_producer_service: EtProducerService = Depends(get_et_producer_service),
    article_images_service: ArticleImagesService = Depends(get_article_images_service),
    s3_service: S3Service = Depends(get_s3_service)
):
    normalized_article = await get_normalized_article(article, articles_service)
    articles = await articles_service.get_articles_by_data_supplier_article_number(normalized_article)
    js_producers = await et_part_service.get_part_by_code(article)

    # Получаем поставщиков из JS
    suppliers_from_js: List[SuppliersSchema] = [
        producer for producer in [
            await et_producer_service.get_producer_by_id(producer.producerId) for producer in js_producers
        ] if producer is not None
    ]


    # Получаем поставщиков из TD
    suppliers_from_td: List[EtProducerSchema] = [
        supplier for supplier in [
            await suppliers_service.get_supplier_by_id(art.supplierId) for art in articles
        ] if supplier is not None
    ]

    # Если поставщики из TD не найдены, пытаемся найти их по имени из JS
    if not suppliers_from_td and suppliers_from_js:
        suppliers_from_td = [
            supplier for supplier in [
                await suppliers_service.get_supplier_by_name(supplier_from_js.name) for supplier_from_js in suppliers_from_js
            ] if supplier is not None
        ]

    await asyncio.gather(*[
        attach_image(supplier_js, "name", normalized_article, et_producer_service, suppliers_service,
                     article_images_service, s3_service)
        for supplier_js in suppliers_from_js
    ])

    await asyncio.gather(*[
        attach_image(supplier_td, "description", normalized_article, et_producer_service, suppliers_service,
                     article_images_service, s3_service)
        for supplier_td in suppliers_from_td
    ])

    # Если ни один из списков не содержит поставщиков, выбрасываем исключение
    if not suppliers_from_td and not suppliers_from_js:
        raise HTTPException(status_code=404, detail="Not found")

    return AllSuppliersSchema(
        suppliersFromJs=suppliers_from_js,
        suppliersFromTd=suppliers_from_td,
    )

