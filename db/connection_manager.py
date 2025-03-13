from sqlalchemy.ext.asyncio import create_async_engine, async_sessionmaker
from sqlalchemy.sql import text

from db.database_enum import DatabaseEnum


class ConnectionManager:
    _engines = {}

    @staticmethod
    def get_session_factory(database: DatabaseEnum):
        if database not in ConnectionManager._engines:
            engine = create_async_engine(database.value, echo=True)
            ConnectionManager._engines[database] = async_sessionmaker(engine, expire_on_commit=False)

        return ConnectionManager._engines[database]

    @staticmethod
    async def execute_sql(database: DatabaseEnum, sql_query: str, parameters: dict = None):
        session_factory = ConnectionManager.get_session_factory(database)
        async with session_factory() as session:
            async with session.begin():
                query = text(sql_query)
                result = await session.execute(query, parameters or {})
                return result.fetchall()
