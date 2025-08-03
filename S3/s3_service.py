import os
import re
from typing import Optional, List
import random
from boto3.session import Session
from botocore.exceptions import ClientError

from S3.s3_setting import S3Setting


class S3Service:
    def __init__(self, s3_setting: S3Setting):
        self.s3_setting = s3_setting
        self.session = Session(
            aws_access_key_id=self.s3_setting.access_key,
            aws_secret_access_key=self.s3_setting.secret_key
        )
        self.s3_client = self.session.client(
            's3',
            endpoint_url=self.s3_setting.endpoint_url,
            region_name=self.s3_setting.region_name
        )

    def get_image_url(self, file_name: str, folder_name: str, expires_in: int = 86400):
        try:
            path = f"{folder_name}/{file_name}"
            self.s3_client.head_object(Bucket=self.s3_setting.bucket_name, Key=path)

            url = self.s3_client.generate_presigned_url(
                'get_object',
                Params={'Bucket': self.s3_setting.bucket_name, 'Key': path},
                ExpiresIn=expires_in,
                HttpMethod='GET'
            )
            return url
        except ClientError:
            return None

    def get_image_view_url(self, base_name: str, folder_name: str = "CTP", expires_in: int = 3600) -> Optional[str]:
        """
        Генерирует URL для просмотра изображения в браузере (автоматически проверяет все форматы)

        :param base_name: Базовое имя файла без расширения (например, "5J0837225B9B9")
        :param folder_name: Папка в S3
        :param expires_in: Время жизни ссылки
        :return: URL первого найденного изображения или None
        """
        # Поддерживаемые форматы изображений
        image_extensions = ['.jpg', '.jpeg', '.png', '.gif', '.webp']

        for ext in image_extensions:
            file_name = f"{base_name}{ext}"
            path = f"{folder_name}/{file_name}"

            try:
                self.s3_client.head_object(Bucket=self.s3_setting.bucket_name, Key=path)

                content_type = {
                    '.jpg': 'image/jpeg',
                    '.jpeg': 'image/jpeg',
                    '.png': 'image/png',
                    '.gif': 'image/gif',
                    '.webp': 'image/webp'
                }[ext]

                url = self.s3_client.generate_presigned_url(
                    'get_object',
                    Params={
                        'Bucket': self.s3_setting.bucket_name,
                        'Key': path,
                        'ResponseContentType': content_type
                    },
                    ExpiresIn=expires_in,
                    HttpMethod='GET'
                )
                return url

            except ClientError as e:
                if e.response['Error']['Code'] != '404':
                    raise

        return None

    def get_image_view_urls(self, base_name: str, folder_name: str = "CTP", expires_in: int = 3600) -> List[str]:
        """
        Генерирует список URL для всех вариантов файла (включая дубли с любой нумерацией),
        с игнорированием регистра и всех символов, кроме букв и цифр.

        :param base_name: Базовое имя файла без расширения
        :param folder_name: Папка в S3
        :param expires_in: Время жизни ссылок
        :return: Список URL всех найденных вариантов
        """

        def normalize(s: str) -> str:
            return re.sub(r'[^a-zA-Z0-9]', '', s).lower()

        urls = []
        normalized_base = normalize(base_name)
        image_extensions = ['.jpg', '.jpeg', '.png', '.gif', '.webp']
        content_type_map = {
            '.jpg': 'image/jpeg',
            '.jpeg': 'image/jpeg',
            '.png': 'image/png',
            '.gif': 'image/gif',
            '.webp': 'image/webp'
        }

        paginator = self.s3_client.get_paginator('list_objects_v2')
        prefix = f"{folder_name}/"

        try:
            for page in paginator.paginate(Bucket=self.s3_setting.bucket_name, Prefix=prefix):
                for obj in page.get('Contents', []):
                    key = obj['Key']
                    filename = key.split('/')[-1]
                    name_part, ext = os.path.splitext(filename)

                    if ext.lower() not in image_extensions:
                        continue

                    normalized_name = normalize(name_part)

                    if normalized_name == normalized_base or normalized_name.startswith(normalized_base):
                        url = self.s3_client.generate_presigned_url(
                            'get_object',
                            Params={
                                'Bucket': self.s3_setting.bucket_name,
                                'Key': key,
                                'ResponseContentType': content_type_map[ext.lower()]
                            },
                            ExpiresIn=expires_in,
                            HttpMethod='GET'
                        )
                        urls.append(url)

        except ClientError as e:
            if e.response['Error']['Code'] != '404':
                raise

        return urls

    def get_image_view_urls_all_folders(self, base_name: str, expires_in: int = 3600) -> List[str]:
        """
        Ищет изображения base_name.{ext} и base_name_*.{ext} в известных папках.
        """
        folders = self.list_folders()
        image_extensions = ['.jpg', '.jpeg', '.png', '.webp']
        content_types = {
            '.jpg': 'image/jpeg',
            '.jpeg': 'image/jpeg',
            '.png': 'image/png',
            '.gif': 'image/gif',
            '.webp': 'image/webp'
        }

        urls = []

        for folder in folders:
            found = False

            for ext in image_extensions:
                key = f"{folder}/{base_name}{ext}"
                try:
                    self.s3_client.head_object(Bucket=self.s3_setting.bucket_name, Key=key)

                    print(f"[DEBUG] Found main file: {key}")

                    url = self.s3_client.generate_presigned_url(
                        'get_object',
                        Params={
                            'Bucket': self.s3_setting.bucket_name,
                            'Key': key,
                            'ResponseContentType': content_types[ext]
                        },
                        ExpiresIn=expires_in,
                        HttpMethod='GET'
                    )
                    urls.append(url)
                    found = True  # Файл найден — можно искать с префиксом

                except ClientError as e:
                    if e.response['Error']['Code'] != '404':
                        raise
                    print(f"[DEBUG] Not found: {key}")

            if not found:
                continue

            prefix = f"{folder}/{base_name}_"
            paginator = self.s3_client.get_paginator('list_objects_v2')
            try:
                for page in paginator.paginate(Bucket=self.s3_setting.bucket_name, Prefix=prefix):
                    for obj in page.get('Contents', []):
                        key = obj['Key']
                        file_name = os.path.basename(key)

                        if not file_name.startswith(f"{base_name}_"):
                            continue

                        ext = os.path.splitext(file_name)[1].lower()
                        if ext not in image_extensions:
                            continue

                        print(f"[DEBUG] Found variant: {key}")

                        url = self.s3_client.generate_presigned_url(
                            'get_object',
                            Params={
                                'Bucket': self.s3_setting.bucket_name,
                                'Key': key,
                                'ResponseContentType': content_types[ext]
                            },
                            ExpiresIn=expires_in,
                            HttpMethod='GET'
                        )
                        urls.append(url)

            except ClientError as e:
                if e.response['Error']['Code'] != '404':
                    raise

        return urls

    def get_random_image_urls(self, count: int, expires_in: int = 3600) -> List[str]:
        """
        Возвращает `count` случайных изображений из случайных папок.
        Останавливается, как только нужное количество найдено.
        """
        folders = self.list_folders()
        random.shuffle(folders)

        image_extensions = ['.jpg', '.jpeg', '.png', '.webp']
        content_types = {
            '.jpg': 'image/jpeg',
            '.jpeg': 'image/jpeg',
            '.png': 'image/png',
            '.gif': 'image/gif',
            '.webp': 'image/webp'
        }

        urls = []

        for folder in folders:
            if len(urls) >= count:
                break

            prefix = f"{folder}/"
            paginator = self.s3_client.get_paginator('list_objects_v2')

            try:
                for page in paginator.paginate(Bucket=self.s3_setting.bucket_name, Prefix=prefix):
                    for obj in page.get('Contents', []):
                        key = obj['Key']
                        ext = os.path.splitext(key)[1].lower()

                        if ext not in image_extensions:
                            continue

                        url = self.s3_client.generate_presigned_url(
                            'get_object',
                            Params={
                                'Bucket': self.s3_setting.bucket_name,
                                'Key': key,
                                'ResponseContentType': content_types.get(ext, 'application/octet-stream')
                            },
                            ExpiresIn=expires_in,
                            HttpMethod='GET'
                        )
                        urls.append(url)

                        if len(urls) >= count:
                            return urls

            except ClientError as e:
                if e.response['Error']['Code'] != '404':
                    raise

        return urls

    def list_folders(self, prefix: str = "") -> List[str]:
        """
        Получает список папок в указанной директории S3

        :param prefix: Префикс пути (например, "CTP/")
        :return: Список имен папок
        """
        try:
            paginator = self.s3_client.get_paginator('list_objects_v2')
            folders = set()

            for page in paginator.paginate(
                    Bucket=self.s3_setting.bucket_name,
                    Prefix=prefix,
                    Delimiter='/'
            ):
                # Добавляем общие префиксы (папки)
                for common_prefix in page.get('CommonPrefixes', []):
                    folder_path = common_prefix['Prefix']
                    folder_name = folder_path[len(prefix):].rstrip('/')
                    if folder_name:  # Исключаем пустые имена
                        folders.add(folder_name)

                # Также проверяем объекты, которые могут представлять "виртуальные" папки
                for obj in page.get('Contents', []):
                    obj_key = obj['Key']
                    if obj_key.endswith('/') and len(obj_key) > len(prefix):
                        folder_name = obj_key[len(prefix):].rstrip('/')
                        if folder_name:
                            folders.add(folder_name)

            return sorted(folders)

        except ClientError as e:
            if e.response['Error']['Code'] == '404':
                return []
            raise