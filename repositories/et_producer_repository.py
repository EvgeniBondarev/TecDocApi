from typing import List

from db.connection_manager import ConnectionManager
from db.database_enum import DatabaseEnum
from models.JCEtalon.et_producers import EtProducer
from repositories.ABC.sql_alchemy_read_repository import SQLAlchemyReadRepository




class EtProducerRepository(SQLAlchemyReadRepository):
    model = EtProducer
    database = DatabaseEnum.JCEtalon

    @staticmethod
    async def get_distinct_market_prefixes() -> List[str]:
        """
        Получает список уникальных значений marketPrefix

        :return: Список уникальных marketPrefix
        """
        query = """
                    SELECT DISTINCT marketPrefix
                    FROM et_producers
                    WHERE marketPrefix IS NOT NULL AND marketPrefix != ''
                    ORDER BY marketPrefix
                """

        result = await ConnectionManager.execute_sql(
            database=DatabaseEnum.JCEtalon,
            sql_query=query
        )

        return [row[0] for row in result if row[0]]