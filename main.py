from fastapi import FastAPI, Depends
from sqlalchemy.ext.asyncio import AsyncSession

from models.JCEtalon.et_producers import EtProducer
from models.TD2018.article_attributes import ArticleAttributes
from models.TD2018.article_images import ArticleImages
from models.TD2018.article_li import ArticleLi
from models.TD2018.articles import Articles
from repositories.article_attributes_repository import ArticlesAttributesRepository
from repositories.article_images_repository import ArticlesImagesRepository
from repositories.article_li_repository import ArticleLiRepository
from repositories.articles_repository import ArticlesRepository
from repositories.et_producer import EtProducerRepository
from schemas.et_producer_schema import EtProducerSchema

app = FastAPI()

et_producer_repo = EtProducerRepository()


@app.get("/et_producer/")
async def root5(
        name: str = "Japan Cars",
):
    et_producer = await et_producer_repo.find(filter_condition=EtProducer.name == name)

    if not et_producer:
        return {"message": "Article not found"}

    tet =  EtProducerSchema.model_validate(et_producer[0])

    return {"et_producer": tet}

@app.get("/hello/{name}")
async def say_hello(name: str):
    return {"message": f"Hello {name}"}
