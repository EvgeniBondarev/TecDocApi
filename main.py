from fastapi import FastAPI
from fastapi.middleware.cors import CORSMiddleware

from routing.substitute_router import router as substitute_router
from routing.article_attributes_router import  router as article_attributes_router
from routing.article_images_router import router as article_images_router
from routing.et_producer_router import router as et_producer_router
from routing.detail_full_info_router import router as detail_full_info_router
from routing.articles_router import router as articles_router
from routing.supplier_router import router as supplier_router
app = FastAPI()

origins = ["*"]

app.add_middleware(
    CORSMiddleware,
    allow_origins=origins,
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

app.include_router(detail_full_info_router)
app.include_router(substitute_router)
app.include_router(supplier_router)
app.include_router(articles_router)
app.include_router(article_attributes_router)
app.include_router(article_images_router)
app.include_router(et_producer_router)

