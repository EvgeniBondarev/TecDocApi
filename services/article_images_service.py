from typing import List

from repositories.article_images_repository import ArticlesImagesRepository
from schemas.aticle_image_schema import ArticleImageSchema


class ArticleImagesService:
    def __init__(self, repository: ArticlesImagesRepository) -> None:
        self.repository = repository

    async def get_all_images(self) -> List[ArticleImageSchema]:
        """Получить все изображения для товаров."""
        records = await self.repository.find_all()
        return [ArticleImageSchema.model_validate(record) for record in records]

    async def get_images_by_filter(self, supplier_id: int, article_number: str) -> List[ArticleImageSchema]:
        """Получить изображения для определенного товара по фильтру."""
        filter_condition = (
            (self.repository.model.supplierId == supplier_id) &
            (self.repository.model.DataSupplierArticleNumber == article_number)
        )
        records = await self.repository.find(filter_condition=filter_condition)
        return [ArticleImageSchema.model_validate(record) for record in records]

    async def get_images_by_supplier(self, supplier_id: int) -> List[ArticleImageSchema]:
        """Получить все изображения для заданного поставщика."""
        filter_condition = self.repository.model.supplierId == supplier_id
        records = await self.repository.find(filter_condition=filter_condition)
        return [ArticleImageSchema.model_validate(record) for record in records]
