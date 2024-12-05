from repositories.article_attributes_repository import ArticlesAttributesRepository
from repositories.article_images_repository import ArticlesImagesRepository
from repositories.et_producer_repository import EtProducerRepository
from repositories.suppliers_repository import SuppliersRepository
from services.article_attributes_service import ArticleAttributesService
from services.article_images_service import ArticleImagesService
from services.et_producer_service import EtProducerService
from services.suppliers_service import SuppliersService


def get_article_attributes_service() -> ArticleAttributesService:
    repository = ArticlesAttributesRepository()
    return ArticleAttributesService(repository)

def get_et_producer_service() -> EtProducerService:
    repository = EtProducerRepository()
    return EtProducerService(repository)

def get_article_images_service() -> ArticleImagesService:
    repository = ArticlesImagesRepository()
    return ArticleImagesService(repository)

def get_suppliers_service() -> SuppliersService:
    repository = SuppliersRepository()
    return SuppliersService(repository)