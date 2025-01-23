from fastapi import APIRouter, Depends


from dependencies import get_et_part_field_data_service, get_et_part_field_service
from schemas.et_part_field_data_schema import EtPartFieldDataSchema
from schemas.et_part_field_schema import EtPartFieldSchema
from services.et_part_field_data_service import EtPartFieldDataService
from services.et_part_field_service import EtPartFieldService
from fastapi import FastAPI, HTTPException

app = FastAPI()

items = {"foo": "The Foo Wrestlers"}


@app.get("/items/{item_id}")
async def read_item(item_id: str):
    if item_id not in items:
        raise HTTPException(status_code=404, detail="Item not found")
    return {"item": items[item_id]}

router = APIRouter(
    prefix="/et_part_field",
    tags=["EtPartsField"]
)

@router.get("/{id}/{producer_id}/", response_model=EtPartFieldDataSchema)
async def get_part_field(id: int, producer_id: int, et_part_service: EtPartFieldService=Depends(get_et_part_field_service),
                                                    et_part_field_data_service: EtPartFieldDataService=Depends(get_et_part_field_data_service)):
    part_field: EtPartFieldSchema = await et_part_service.get_partfields_by_id_and_producer(id, producer_id)
    if part_field is not None:
        return await et_part_field_data_service.get_data_by_id(int(part_field.dataid))
    raise HTTPException(status_code=404, detail="Item not found")