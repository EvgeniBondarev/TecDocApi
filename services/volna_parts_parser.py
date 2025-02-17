import requests
from bs4 import BeautifulSoup
from pydantic import BaseModel
from typing import List, Dict

from schemas.part_data_shema import PartDataSchema
from schemas.replacement_part_schema import ReplacementPartSchema


class VolnaPartsParser:
    @staticmethod
    def parse_part(article: str) -> PartDataSchema:
        url = f"https://volna.parts/search/number/?article={article}"
        response = requests.get(url)
        soup = BeautifulSoup(response.text, 'html.parser')

        # Парсинг названия
        name_tag = soup.find('h1', {'id': 'ex1'})
        name = name_tag.text.strip() if name_tag else ''

        # Парсинг изображений
        images = []
        image_div = soup.find('div', class_='card__main_image')
        if image_div:
            img_tags = image_div.find_all('img')
            images = [img['src'] for img in img_tags if img.has_attr('src')]

        # Парсинг характеристик
        specs = {}
        spec_div = soup.find('div', class_='card__specifications')
        if spec_div:
            params = spec_div.find_all('li', class_='card-footer__param')
            for param in params:
                key_span = param.find('div', class_='card-param__firts').find('span')
                value_span = param.find('div', class_='card-param__sec').find('span')

                if key_span and value_span:
                    key = key_span.text.strip().rstrip(':')
                    value = value_span.text.strip()
                    specs[key] = value

        replacements = []
        replacements_div = soup.find('div', class_='t7 dsr-container result-procenka')
        if replacements_div:
            for item in replacements_div.find_all('div', class_=lambda x: x and x.startswith('ftr second')):
                try:
                    # Бренд
                    brand_tag = item.find('span', class_='g-brand-to-find')
                    brand = brand_tag['data-brand'] if brand_tag and brand_tag.has_attr(
                        'data-brand') else brand_tag.text.strip() if brand_tag else ''

                    # Артикул
                    article_tag = item.find('span', class_='article-field-active')
                    if article_tag:
                        article_link = article_tag.find('a', class_='hover-dec')
                        article = article_link.text.strip() if article_link else ''
                    else:
                        article = ''

                    # Изображение
                    img_tag = item.find('img', class_='lazy')
                    image = img_tag['data-src'] if img_tag and img_tag.has_attr('data-src') else img_tag[
                        'src'] if img_tag else None

                    # Цена
                    price_tag = item.find('span', class_='price-now')
                    price = float(price_tag.text.strip().replace(',', '.')) if price_tag else None

                    # Количество
                    quantity_tag = item.find('div', class_='gb-7-none')
                    quantity = int(quantity_tag.text.split()[0]) if quantity_tag else None

                    # Доставка
                    delivery_tag = item.find('div', class_='g-delivery')
                    delivery = ' '.join(delivery_tag.stripped_strings) if delivery_tag else ''

                    replacements.append(ReplacementPartSchema(
                        brand=brand,
                        article=article,
                        image=image,
                        price=price,
                        quantity=quantity,
                        delivery=delivery
                    ))
                except Exception as e:
                    print(f"Error parsing replacement: {e}")
                    continue

        return PartDataSchema(
            name=name,
            images=images,
            specifications=specs,
            replacements=replacements
        )