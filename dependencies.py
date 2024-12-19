from S3.s3_service import S3Service
from S3.s3_setting import S3Setting
from config import S3_ACCESS_KEY, S3_SECRET_KEY, S3_BUCKET_NAME, S3_REGION_NAME, S3_ENDPOINT_URL
from repositories.article_attributes_repository import ArticlesAttributesRepository
from repositories.article_ean_repository import ArticleEANRepository
from repositories.article_images_repository import ArticlesImagesRepository
from repositories.articles_repository import ArticlesRepository
from repositories.et_producer_repository import EtProducerRepository
from repositories.suppliers_repository import SuppliersRepository
from repositories.utils.substitute_finder import SubstituteFinder
from services.article_attributes_service import ArticleAttributesService
from services.article_ean_service import ArticleEANService
from services.article_images_service import ArticleImagesService
from services.articles_service import ArticlesService
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

def get_articles_service() -> ArticlesService:
    repository = ArticlesRepository()
    return ArticlesService(repository)

def get_article_ean_service() -> ArticleEANService:
    repository = ArticleEANRepository()
    return ArticleEANService(repository)

def get_s3_service() -> S3Service:
    return S3Service(S3Setting(S3_ACCESS_KEY,
                               S3_SECRET_KEY,
                               S3_ENDPOINT_URL,
                               S3_REGION_NAME,
                               S3_BUCKET_NAME))

def get_substitute_finder_service() -> SubstituteFinder:
    return SubstituteFinder()