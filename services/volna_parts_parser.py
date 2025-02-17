import requests
from bs4 import BeautifulSoup
from typing import List
from schemas.part_data_shema import PartDataSchema
from schemas.replacement_part_schema import ReplacementPartSchema
from urllib.parse import unquote


class VolnaPartsParser:
    @staticmethod
    def parse_part(article: str) -> List[PartDataSchema]:
        url = f"https://volna.parts/search/number/?article={article}"
        try:
            response = requests.get(url)
            response.raise_for_status()
        except requests.RequestException as e:
            print(f"Request error: {e}")
            return []

        soup = BeautifulSoup(response.text, 'html.parser')
        artlookup_div = soup.find('div', class_='details-list artlookup-wrap')

        if artlookup_div:
            brand_links = []
            for ftr_div in artlookup_div.find_all('div', class_='ftr cursor'):
                brand_descr = ftr_div.find('div', class_=lambda x: x and 'g-brand2' in x)
                if brand_descr:
                    a_tag = brand_descr.find('a')
                    if a_tag and a_tag.has_attr('href'):
                        link = a_tag['href']
                        brand_links.append(link)

            parts = []
            for link in brand_links:
                try:
                    response = requests.get(link)
                    response.raise_for_status()
                    part_data = VolnaPartsParser._parse_part_page(response)
                    parts.append(part_data)
                except Exception as e:
                    print(f"Error parsing {link}: {e}")
                    continue
            return parts
        else:
            part_data = VolnaPartsParser._parse_part_page(response)
            return [part_data]

    @staticmethod
    def _parse_part_page(response) -> PartDataSchema:
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

        # Парсинг замен с проверкой уникальности
        replacements = []
        seen_replacements = set()  # Для отслеживания уникальных комбинаций
        replacement_containers = soup.find_all('div', class_=lambda x: x and 'dsr-container' in x)

        for container in replacement_containers:
            items = container.find_all('div', class_=lambda x: x and ('ftr' in x or 'g-ftr-box' in x))

            for item in items:
                try:
                    # Парсинг бренда
                    brand = ''
                    brand_tag = item.find('span', class_='g-brand-to-find')
                    if brand_tag:
                        brand = brand_tag.get('data-brand', brand_tag.text.strip())
                    if not brand:
                        brand_div = item.find('div', class_='g-brand')
                        if brand_div:
                            brand = brand_div.get_text(strip=True)
                    brand = unquote(brand).strip()

                    # Парсинг артикула
                    article = ''
                    article_tags = [
                        item.find('span', class_='article-field-active'),
                        item.find('div', class_='cat-descr')
                    ]

                    for tag in article_tags:
                        if not tag or article:
                            continue
                        if tag.name == 'span':
                            article_link = tag.find('a')
                            article = article_link.text.strip() if article_link else tag.text.strip()
                        elif tag.name == 'div':
                            article = tag.find('a').text.split()[0].strip()

                    article = article.strip()

                    # Проверка на минимальную валидность данных
                    if not brand or not article:
                        continue

                    # Создание уникального ключа
                    unique_key = f"{brand.lower()}|{article.lower()}".replace(' ', '')

                    # Пропуск дубликатов
                    if unique_key in seen_replacements:
                        continue
                    seen_replacements.add(unique_key)

                    # Парсинг изображения
                    image = None
                    img_tag = item.find('img', {'src': True})
                    if not img_tag:
                        img_tag = item.find('img', {'data-src': True})
                    if img_tag:
                        image = img_tag.get('data-src') or img_tag.get('src')

                    # Парсинг цены
                    price = None
                    price_tags = item.find_all(['span', 'div'], class_=['price-now', 'wp-price_summ'])
                    for tag in price_tags:
                        if price:
                            break
                        try:
                            price_text = tag.text.split('р.')[0].replace(',', '.').strip()
                            price = float(price_text)
                        except (ValueError, AttributeError):
                            continue

                    # Парсинг количества
                    quantity = None
                    quantity_sources = [
                        item.find('div', class_='gb-7-none'),
                        item.find('span', class_='wp_basket_stock_title')
                    ]
                    for source in quantity_sources:
                        if quantity or not source:
                            continue
                        try:
                            quantity_text = source.text.split('шт')[0].strip()
                            quantity = int(quantity_text)
                        except (ValueError, AttributeError):
                            continue

                    # Парсинг доставки
                    delivery = ''
                    delivery_sources = [
                        item.find('div', class_='g-delivery'),
                        item.find('div', class_='wp_dellivery__el')
                    ]
                    for source in delivery_sources:
                        if delivery or not source:
                            continue
                        delivery = ' '.join(source.stripped_strings)

                    replacements.append(ReplacementPartSchema(
                        brand=brand,
                        article=article,
                        image=image,
                        price=price,
                        quantity=quantity,
                        delivery=delivery
                    ))

                except Exception as e:
                    print(f"Error parsing replacement item: {e}")
                    continue

        return PartDataSchema(
            name=name,
            images=images,
            specifications=specs,
            replacements=replacements
        )