from typing import List

from S3.s3_service import S3Service

from fastapi import APIRouter, Depends, HTTPException, UploadFile, File
from dependencies import get_s3_service



router = APIRouter(prefix="/s3", tags=["S3 Operations"])


# Роутер для работы с изображениями
@router.get("/image-url")
async def get_image_url(
        file_name: str,
        folder_name: str = "CTP",
        s3_service: S3Service = Depends(get_s3_service)
):
    """
    Получить временную ссылку на изображение из S3

    - **file_name**: Имя файла
    - **folder_name**: Папка в S3 (по умолчанию 'CTP')
    """
    url = s3_service.get_image_url(file_name, folder_name)
    if not url:
        raise HTTPException(status_code=404, detail="File not found")
    return {"url": url}

@router.get("/image-view-urls", response_model=List[str])
async def get_image_url(
        code: str,
        folder_name: str = "CTP",
        s3_service: S3Service = Depends(get_s3_service)
):
    """
    Получить временную ссылку на изображение из S3

    - **file_name**: Имя файла
    - **folder_name**: Папка в S3 (по умолчанию 'CTP')
    """
    urls = s3_service.get_image_view_urls(code, folder_name)
    if not urls:
        raise HTTPException(status_code=404, detail="File not found")
    return urls


@router.get("/folders", response_model=List[str])
async def list_s3_folders(
        prefix: str = "",
        s3_service: S3Service = Depends(get_s3_service)
):
    """
    Получить список папок в S3 хранилище

    - **prefix**: Необязательный префикс пути (например, "CTP/")
    - **response**: Список имен папок
    """
    return s3_service.list_folders(prefix)

