from fastapi import APIRouter, Depends, HTTPException

from dependencies import get_articles_service, get_suppliers_service, get_et_part_service, get_et_producer_service
from schemas.all_suppliers_schema import AllSuppliersSchema
from services.articles_service import ArticlesService
from services.et_part_service import EtPartService
from services.et_producer_service import EtProducerService
from services.suppliers_service import SuppliersService
from services.utils.search_preparation import get_normalized_article

router = APIRouter(
    prefix="/suppliers",
    tags=["Supplier"]
)


@router.get("/{article}", response_model=AllSuppliersSchema)
async def get_suppliers_by_article(article: str, articles_service : ArticlesService=Depends(get_articles_service),
                                                 suppliers_service: SuppliersService = Depends(get_suppliers_service),
                                                 et_part_service: EtPartService = Depends(get_et_part_service),
                                                 et_producer_service: EtProducerService = Depends(get_et_producer_service)):
    normalized_article = await get_normalized_article(article, articles_service)

    articles = await articles_service.get_articles_by_data_supplier_article_number(normalized_article)
    js_producers = await  et_part_service.get_part_by_code(article)

    suppliers_from_td = [
        supplier for supplier in [
            await suppliers_service.get_supplier_by_id(art.supplierId) for art in articles
        ] if supplier is not None
    ]

    suppliers_from_js = [
        producer for producer in [
            await et_producer_service.get_producer_by_id(producer.producerId) for producer in js_producers
        ] if producer is not None
    ]

    if len(suppliers_from_td) == 0 or len(suppliers_from_js) == 0:
        raise HTTPException(status_code=404, detail="Not found")

    return AllSuppliersSchema(
        suppliersFromJs=suppliers_from_js,
        suppliersFromTd=suppliers_from_td,
    )


