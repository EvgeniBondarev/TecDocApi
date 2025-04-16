import base64

import requests
from bs4 import BeautifulSoup
from typing import List
from schemas.part_data_shema import PartDataSchema
from schemas.replacement_part_schema import ReplacementPartSchema
from urllib.parse import unquote

from schemas.substitute.attribute_schema import Attribute
from schemas.volva.applicability.vehicle_manufacturer import VehicleManufacturer
from schemas.volva.applicability.vehicle_model import VehicleModel
from schemas.volva.applicability.vehicle_modification import VehicleModification
from schemas.volva.applicability.vehicle_specification import VehicleSpecification
from schemas.volva.cross_numberItem import CrossNumberItem
from schemas.volva.manufacturer_cross_numbers import ManufacturerCrossNumbers
from schemas.volva.volna_part import VolnaPart


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
    def parse_part_by_brand(article: str, brand: int) -> List[PartDataSchema]:
        url = f"https://volna.parts/search/number/?article={article}&brand={brand}"
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

    @staticmethod
    def parse_part_details(article: str, brand: int = None) -> List[VolnaPart]:
        """Parse part details directly from the secondary page (linked from image)"""
        # Сначала получаем URL второй страницы из основной
        main_url = f"https://volna.parts/search/number/?article={article}"
        if brand is not None:
            main_url += f"&brand={brand}"

        try:
            response = requests.get(main_url)
            response.raise_for_status()
        except requests.RequestException as e:
            print(f"Request error: {e}")
            return []

        soup = BeautifulSoup(response.text, 'html.parser')

        # Находим ссылку на вторую страницу в картинке
        image_div = soup.find('div', class_='card__main_image')
        if not image_div:
            return []  # Если нет картинки, значит нет и второй страницы

        a_tag = image_div.find('a', class_='main-photo')
        if not a_tag or not a_tag.has_attr('href'):
            return []

        detail_url = a_tag['href']

        # Парсим сразу вторую страницу
        try:
            detail_response = requests.get(detail_url)
            detail_response.raise_for_status()
            part_data = VolnaPartsParser._parse_detail_page(detail_response)
            return [part_data]
        except Exception as e:
            print(f"Error parsing detail page {detail_url}: {e}")
            return []

    @staticmethod
    def _parse_detail_page(response) -> VolnaPart:
        """Парсинг детальной страницы (второй страницы)"""
        soup = BeautifulSoup(response.text, 'html.parser')

        # Парсим название
        name_tag = soup.find('h1', {'id': 'ex1'})
        name = name_tag.text.strip() if name_tag else ''

        # Парсим изображения товара
        images = []
        image_div = soup.find('div', class_='card__main_image')
        if image_div:
            img_tags = image_div.find_all('img')
            images = [img['src'] for img in img_tags if img.has_attr('src')]
            if not images:
                images = [img['data-src'] for img in img_tags if img.has_attr('data-src')]

        # Парсим картинку производителя
        manufacturer_image = None
        manufacturer_div = soup.find('div', class_='card-links__img')
        if manufacturer_div:
            img_tag = manufacturer_div.find('img')
            if img_tag and img_tag.has_attr('src'):
                manufacturer_image = img_tag['src']

        # Парсим атрибуты
        attributes = []
        spec_div = soup.find('div', class_='card__specifications')
        if spec_div:
            params = spec_div.find_all('li', class_='card-footer__param')
            for param in params:
                key_div = param.find('div', class_='card-param__firts')
                value_div = param.find('div', class_='card-param__sec')
                if key_div and value_div:
                    key_span = key_div.find('span')
                    value_span = value_div.find('span')
                    if key_span and value_span:
                        title = key_span.text.strip().rstrip(':')
                        value = value_span.text.strip()
                        attributes.append(Attribute(Title=title, Value=value))

        # Парсим характеристики
        characteristics = []
        characteristics_block = soup.find('div', class_='footer-content__block card-param__with__dots')
        if characteristics_block:
            params = characteristics_block.find_all('li', class_='card-footer__param')
            for param in params:
                key_div = param.find('div', class_='card-param__firts')
                value_div = param.find('div', class_='card-param__sec')
                if key_div and value_div:
                    key_span = key_div.find('span')
                    if key_span:
                        title = key_span.text.strip().rstrip(':')
                        value_spans = value_div.find_all('span')
                        values = [span.text.strip() for span in value_spans
                                  if 'param-secdots__del' not in span.get('class', [])]
                        value = ' / '.join(values).strip()
                        if title and value:
                            characteristics.append(Attribute(Title=title, Value=value))

            # Парсим кросс-номера
        cross_numbers = []
        cross_numbers_block = soup.find('div', id='orignumbers')
        if cross_numbers_block:
            manufacturer_blocks = cross_numbers_block.find_all('div', class_='card-orignumbers__el')
            for block in manufacturer_blocks:
                # Парсим название производителя
                manufacturer_div = block.find('div', class_='card-orignumbers__article')
                manufacturer = manufacturer_div.text.strip() if manufacturer_div else 'Unknown'

                # Парсим номера
                numbers = []
                numbers_list = block.find('ul', class_='card-orignumbers__ul')
                if numbers_list:
                    for item in numbers_list.find_all('li', class_='card-param__param'):
                        number = item.text.strip()
                        search_link = None
                        if item.has_attr('data-link'):
                            try:
                                # Декодируем base64 ссылку
                                decoded_link = base64.b64decode(item['data-link']).decode('utf-8')
                                search_link = f"https://volna.parts{decoded_link}"
                            except:
                                search_link = None

                        numbers.append(CrossNumberItem(
                            number=number,
                            search_link=search_link
                        ))

                if manufacturer and numbers:
                    cross_numbers.append(ManufacturerCrossNumbers(
                        manufacturer=manufacturer,
                        numbers=numbers
                    ))


        return VolnaPart(
            name=name,
            images=images,
            attributes=attributes,
            characteristics=characteristics,
            manufacturer_image=manufacturer_image,
            cross_numbers=cross_numbers,
        )


    @staticmethod
    def _parse_modifications(article_id: int, manu_id: int, model_id: int) -> List[VehicleModification]:
        """Парсинг модификаций по AJAX-запросу"""
        url = f"https://volna.parts/detail/types/?article_id={article_id}&manu_id={manu_id}&model_id={model_id}&type=auto"
        try:
            response = requests.get(url)
            response.raise_for_status()
        except requests.RequestException as e:
            print(f"Request error for modifications: {e}")
            return []

        soup = BeautifulSoup(response.text, 'html.parser')
        modifications = []

        table_body = soup.find('div', class_='table-body')
        if not table_body:
            return modifications

        for row in table_body.find_all('div', class_='table-row'):
            try:
                # Основные данные
                name = row.find('span', class_='table-link').text.strip() if row.find('span', class_='table-link') else ""
                power = row.find('a', title=lambda x: x and 'л.с.' in x).text.strip() if row.find('a', title=lambda
                    x: x and 'л.с.' in x) else None
                engine = row.find('a', title=lambda x: x and ('Бензин' in x or 'Дизель' in x)).text.strip() if row.find('a',
                                                                                                                        title=lambda
                                                                                                                            x: x and (
                                                                                                                                    'Бензин' in x or 'Дизель' in x)) else None
                volume = row.find('a', title=lambda x: x and 'см' in x).text.strip() if row.find('a', title=lambda
                    x: x and 'см' in x) else None
                drive = row.find('a', title=lambda x: x and ('Привод' in x or 'колеса' in x)).text.strip() if row.find('a',
                                                                                                                       title=lambda
                                                                                                                           x: x and (
                                                                                                                                   'Привод' in x or 'колеса' in x)) else None
                period = row.find('a',
                                  title=lambda x: x and ('—' in x or any(c.isdigit() for c in x))).text.strip() if row.find(
                    'a', title=lambda x: x and ('—' in x or any(c.isdigit() for c in x))) else None

                # Дополнительные характеристики
                specs = []
                hidden_block = row.find('div', class_='table-row__hidden')
                if hidden_block:
                    for wrap in hidden_block.find_all('div', class_='row-hidden__wrap'):
                        title = wrap.find('div', class_='row-hidden__tit').text.strip() if wrap.find('div',
                                                                                                     class_='row-hidden__tit') else None
                        value = wrap.find('div', class_='row-hidden__content').text.strip() if wrap.find('div',
                                                                                                         class_='row-hidden__content') else None
                        if title and value:
                            specs.append(VehicleSpecification(name=title, value=value))

                modifications.append(VehicleModification(
                    name=name,
                    power=power,
                    engine_type=engine,
                    engine_volume=volume,
                    drive_type=drive,
                    production_period=period,
                    specifications=specs
                ))
            except Exception as e:
                print(f"Error parsing modification: {e}")
                continue

        return modifications