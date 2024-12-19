from typing import List

from db.connection_manager import ConnectionManager
from db.database_enum import DatabaseEnum
from schemas.substitute.attribute_schema import Attribute
from schemas.substitute.modification_schema import Modification
from schemas.substitute.substitute_schema import SubstituteSchema


class SubstituteFinder:
    application_group_map = {
        2: {"table": "passanger_car", "id_field": "passangercarid", "name": "Легковой автомобиль"},
        16: {"table": "commercial_vehicle", "id_field": "commertialvehicleid", "name": "Коммерческий автомобиль"},
        777: {"table": "motorbike", "id_field": "motorbikeid", "name": "Мотоцикл"},
        14: {"table": "engine", "id_field": "engineid", "name": "Двигатель"},
        19: {"table": "axle", "id_field": "axleid", "name": "Ось"},
    }

    @staticmethod
    async def find_substitute(article: str, supplier_id: int) -> List[SubstituteSchema]:
        results = []

        article_links_result = await SubstituteFinder._fetch_article_links(article, supplier_id)

        for article_link in article_links_result:
            supplier_id, product_id, group_type, entity_id = map(int, article_link[:4])

            group_info = SubstituteFinder.application_group_map.get(group_type)
            if not group_info:
                continue

            table = group_info["table"]
            id_field = group_info["id_field"]

            pds_result = await SubstituteFinder._fetch_pds(table, id_field, entity_id, product_id, supplier_id)
            for pd in pds_result:
                trees_result = await SubstituteFinder._fetch_trees(table, id_field, pd)
                modifications_result = await SubstituteFinder._fetch_modifications(table, pd)
                attributes_result = await SubstituteFinder._fetch_attributes(table, id_field, pd)

                result = SubstituteFinder._transform_to_result(
                    article_link, trees_result, modifications_result, attributes_result
                )
                results.append(result)

        return results

    @staticmethod
    async def _fetch_article_links(article: str, supplier_id: int):
        query = (
            "SELECT * FROM article_links "
            f"WHERE datasupplierarticlenumber = '{article}' AND supplierid = {supplier_id}"
        )
        return await ConnectionManager.execute_sql(DatabaseEnum.TD2018, query)

    @staticmethod
    async def _fetch_pds(table: str, id_field: str, entity_id: int, product_id: int, supplier_id: int):
        query = (
            f"SELECT * FROM {table}_pds "
            f"WHERE {id_field} = {entity_id} AND productid = {product_id} AND supplierid = {supplier_id}"
        )
        return await ConnectionManager.execute_sql(DatabaseEnum.TD2018, query)

    @staticmethod
    async def _fetch_trees(table: str, id_field: str, pd: tuple):
        query = f"SELECT * FROM {table}_trees WHERE {id_field} = %s AND id = %s"
        return await ConnectionManager.execute_sql(DatabaseEnum.TD2018, query % (int(pd[0]), int(pd[1])))

    @staticmethod
    async def _fetch_modifications(table: str, pd: tuple):
        query = f"SELECT * FROM {table}s WHERE id = %s"
        return await ConnectionManager.execute_sql(DatabaseEnum.TD2018, query % int(pd[1]))

    @staticmethod
    async def _fetch_attributes(table: str, id_field: str, pd: tuple):
        query = f"SELECT * FROM {table}_attributes WHERE {id_field} = %s"
        return await ConnectionManager.execute_sql(DatabaseEnum.TD2018, query % int(pd[0]))

    @staticmethod
    def _transform_to_result(article_link: tuple, trees_result: list, modifications_result: list, attributes_result: list) -> SubstituteSchema:
        modification = Modification(
            description=modifications_result[0][4],
            construction_interval=modifications_result[0][2]
        )

        attributes = [
            Attribute(Title=attr[3], Value=attr[4])
            for attr in attributes_result
        ]

        return SubstituteSchema(
            Type= SubstituteFinder.application_group_map[article_link[2]]["name"],
            Name= trees_result[0][4],
            Modification= modification,
            Attributes= attributes
        )
