from fastapi import APIRouter, Depends, HTTPException
from typing import List, Optional

from dependencies import get_substitute_finder_service, get_et_producer_service, get_articles_service, \
    get_suppliers_service
from schemas.substitute.substitute_result_schema import SubstituteResultSchema

from services.articles_service import ArticlesService
from services.et_producer_service import EtProducerService
from services.suppliers_service import SuppliersService
from services.utils.search_preparation import get_supplier_id, get_normalized_article

router = APIRouter(
    prefix="/substitute",
    tags=["Substitute"]
)


@router.get("/{supplier}/{article}", response_model=SubstituteResultSchema)
async def find_substitute(
        supplier: str,
        article: str,
        et_producer_service: EtProducerService = Depends(get_et_producer_service),
        articles_service: ArticlesService = Depends(get_articles_service),
        suppliers_service: SuppliersService = Depends(get_suppliers_service),
        service=Depends(get_substitute_finder_service)
):
    normalized_article = await get_normalized_article(article, articles_service)
    supplier_id = await get_supplier_id(supplier, et_producer_service, suppliers_service)

    try:
        substitutes = await service.find_substitute(normalized_article, supplier_id)
        return SubstituteResultSchema(
            Models=substitutes,
            SubstitutesCount=sum(len(model.Substitutes) for model in substitutes)
        )
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))

