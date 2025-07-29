import asyncio
from typing import List

from fastapi import APIRouter, Depends, HTTPException

from S3.s3_service import S3Service
from dependencies import get_articles_service, get_suppliers_service, get_et_part_service, get_et_producer_service, \
    get_article_images_service, get_s3_service, get_supplier_details_service
from schemas.all_suppliers_schema import AllSuppliersSchema
from schemas.et_producer_schema import EtProducerSchema
from schemas.supplier_details_schema import SupplierDetailsSchema
from schemas.suppliers_schema import SuppliersSchema
from services.article_images_service import ArticleImagesService
from services.articles_service import ArticlesService
from services.et_part_service import EtPartService
from services.et_producer_service import EtProducerService
from services.supplier_details_service import SupplierDetailsService
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
    # Параллельное выполнение основных запросов
    normalized_article, articles, js_producers = await asyncio.gather(
        get_normalized_article(article, articles_service),
        articles_service.get_articles_by_data_supplier_article_number(article),
        et_part_service.get_part_by_code(article)
    )

    # Получаем уникальные producerId из js_producers
    producer_ids = {p.producerId for p in js_producers if p.producerId}

    # Параллельно получаем всех производителей из JS
    suppliers_from_js_tasks = [
        et_producer_service.get_producer_by_id(producer_id)
        for producer_id in producer_ids
    ]
    suppliers_from_js = [
        producer for producer in await asyncio.gather(*suppliers_from_js_tasks)
        if producer is not None
    ]

    # Получаем уникальные supplierId из articles
    supplier_ids = {art.supplierId for art in articles if art.supplierId}

    # Параллельно получаем всех поставщиков из TD
    suppliers_from_td_tasks = [
        suppliers_service.get_supplier_by_id(supplier_id)
        for supplier_id in supplier_ids
    ]
    suppliers_from_td = [
        supplier for supplier in await asyncio.gather(*suppliers_from_td_tasks)
        if supplier is not None
    ]

    # Если поставщики из TD не найдены, пытаемся найти их по имени из JS
    if not suppliers_from_td and suppliers_from_js:
        supplier_names = {s.name for s in suppliers_from_js}
        suppliers_from_td_tasks = [
            suppliers_service.get_supplier_by_name(name)
            for name in supplier_names
        ]
        suppliers_from_td = [
            supplier for supplier in await asyncio.gather(*suppliers_from_td_tasks)
            if supplier is not None
        ]

    # Параллельная загрузка изображений
    image_tasks = []
    for supplier_js in suppliers_from_js:
        image_tasks.append(
            attach_image(supplier_js, "name", normalized_article, et_producer_service,
                         suppliers_service, article_images_service, s3_service)
        )
    for supplier_td in suppliers_from_td:
        image_tasks.append(
            attach_image(supplier_td, "description", normalized_article, et_producer_service,
                         suppliers_service, article_images_service, s3_service)
        )

    await asyncio.gather(*image_tasks)

    if not suppliers_from_td and not suppliers_from_js:
        raise HTTPException(status_code=404, detail="Not found")

    return AllSuppliersSchema(
        suppliersFromJs=suppliers_from_js,
        suppliersFromTd=suppliers_from_td,
    )

@router.get("/by-id/{supplier_id}", response_model=List[SupplierDetailsSchema])
async def get_supplier_details_by_id(
    supplier_id: int,
    supplier_details_service: SupplierDetailsService = Depends(get_supplier_details_service)
):
    """
    Получить все детали поставщика по его ID
    """
    details = await supplier_details_service.get_details_by_supplier_id(supplier_id)
    if not details:
        raise HTTPException(
            status_code=404,
            detail=f"Details for supplier with ID {supplier_id} not found"
        )
    return details