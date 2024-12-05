from fastapi import FastAPI

from routing.article_attributes_router import  router as article_attributes_router
from routing.article_images_router import router as article_images_router
from routing.et_producer_router import router as et_producer_router
from routing.detail_full_info_router import router as detail_full_info_router
app = FastAPI()

app.include_router(detail_full_info_router)
app.include_router(article_attributes_router)
app.include_router(article_images_router)
app.include_router(et_producer_router)