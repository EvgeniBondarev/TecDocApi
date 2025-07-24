import uvicorn
from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware

from routing.img_router import router as img_router
from routing.substitute_router import router as substitute_router
from routing.article_attributes_router import  router as article_attributes_router
from routing.article_images_router import router as article_images_router
from routing.et_producer_router import router as et_producer_router
from routing.detail_full_info_router import router as detail_full_info_router
from routing.articles_router import router as articles_router
from routing.supplier_router import router as supplier_router
from routing.et_part_router import router as et_part_router
from routing.et_part_field_router import router as et_part_field_router
from routing.et_string_router import router as et_string_router
from routing.pr_part_router import router as pr_part_router
from routing.cr_t_cross_router import router as cr_t_cross_router
from routing.volna_parts_router import router as volna_parts_router
from routing.tec_doc_cross_router import router as tec_dac_cross_router
from routing.product_information_router import router as product_information_router
app = FastAPI(
    title="Parts Info API",
    description="https://interparts.mpsuper.ru/PartsInfo/Images, "
                "https://interparts.mpsuper.ru/PartsInfo/Info/CodeInfo",
    version="5.5"
)
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],  # Разрешаем доступ с любых доменов
    allow_credentials=True,
    allow_methods=["*"],  # Разрешаем все HTTP-методы
    allow_headers=["*"],  # Разрешаем все заголовки
)
app.include_router(detail_full_info_router)
app.include_router(substitute_router)
app.include_router(supplier_router)
app.include_router(articles_router)
app.include_router(article_attributes_router)
app.include_router(article_images_router)
app.include_router(et_producer_router)
app.include_router(et_part_router)
app.include_router(et_part_field_router)
app.include_router(et_string_router)
app.include_router(pr_part_router)
app.include_router(cr_t_cross_router)
app.include_router(volna_parts_router)
app.include_router(tec_dac_cross_router)
app.include_router(img_router)
app.include_router(product_information_router)
