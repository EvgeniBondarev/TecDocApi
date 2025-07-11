from typing import List

from S3.s3_service import S3Service
from fastapi import APIRouter, Depends, HTTPException, UploadFile, File
from dependencies import get_s3_service, get_et_producer_service, get_duckduckgo_image_service
from schemas.et_producer_schema import EtProducerSchema
from services.duckduckgo_img_parser import DuckDuckGoImageSearch
from services.et_producer_service import EtProducerService

router = APIRouter(prefix="/s3", tags=["S3 Operations"])


@router.get("/image-url")
async def get_image_url(
        file_name: str,
        folder_name: str = "CTP",
        s3_service: S3Service = Depends(get_s3_service)
):
    url = s3_service.get_image_url(file_name, folder_name)
    if not url:
        raise HTTPException(status_code=404, detail="File not found")
    return {"url": url}

@router.get("/image-url-by-producer-name")
async def get_image_url(
        code: str,
        producer_name: str,
        s3_service: S3Service = Depends(get_s3_service),
        et_producer_service: EtProducerService = Depends(get_et_producer_service)
):
    producer: EtProducerSchema = await et_producer_service.get_producers_by_name(producer_name)
    if producer and producer.marketPrefix != "":
        url = s3_service.get_image_view_urls(code, producer.marketPrefix)
    else:
        raise HTTPException(status_code=404, detail="Producer not found")

    if not url:
        raise HTTPException(status_code=404, detail="File not found")
    return {"url": url}

@router.get("/image-url-by-producer-id")
async def get_image_url(
        code: str,
        producer_id: str,
        s3_service: S3Service = Depends(get_s3_service),
        et_producer_service: EtProducerService = Depends(get_et_producer_service)
):
    producer: EtProducerSchema = await et_producer_service.get_producer_by_id(producer_id)
    if producer and producer.marketPrefix != "":
        url = s3_service.get_image_view_urls(code, producer.marketPrefix )
    else:
        raise HTTPException(status_code=404, detail="Producer not found")

    if not url:
        raise HTTPException(status_code=404, detail="File not found")
    return {"url": url}


@router.get("/image-view-urls", response_model=List[str])
async def get_image_url(
        code: str,
        folder_name: str = "CTP",
        s3_service: S3Service = Depends(get_s3_service)
):
    urls = s3_service.get_image_view_urls(code, folder_name)
    if not urls:
        raise HTTPException(status_code=404, detail="File not found")
    return urls


@router.get("/image-view-urls-all", response_model=List[str])
async def get_image_urls_all_folders(
        code: str,
        s3_service: S3Service = Depends(get_s3_service)
):
    """
    Получить временные ссылки на изображения из всех папок S3 по коду (имени файла без расширения)

    - **code**: Базовое имя файла
    """
    urls = s3_service.get_image_view_urls_all_folders(code)
    if not urls:
        raise HTTPException(status_code=404, detail="Files not found")
    return urls

@router.get("/random-images", response_model=List[str])
async def get_random_images(
        count: int = 5,
        s3_service: S3Service = Depends(get_s3_service)
):
    """
    Получить указанное количество случайных изображений из всех папок в S3.

    - **count**: Количество изображений
    """
    urls = s3_service.get_random_image_urls(count)
    if not urls:
        raise HTTPException(status_code=404, detail="No images found")
    return urls


@router.get("/duck-duck-go-search-images", response_model=List[str])
async def search_images(
        query: str,
        count: int = 5,
        image_service: DuckDuckGoImageSearch = Depends(get_duckduckgo_image_service)
):
    """
    Поиск изображений через DuckDuckGo

    - **query**: Поисковый запрос
    - **count**: Количество изображений (макс. 50)
    """
    if count > 50:
        raise HTTPException(status_code=400, detail="Максимальное количество изображений - 50")

    images = image_service.search_images(query, min(count, 50))
    if not images:
        raise HTTPException(status_code=404, detail="Изображения не найдены")
    return images


@router.get("/folders", response_model=List[str])
async def list_s3_folders(
        prefix: str = "",
        s3_service: S3Service = Depends(get_s3_service)
):
    return s3_service.list_folders(prefix)
