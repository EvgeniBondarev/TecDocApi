from sqlalchemy.ext.asyncio import create_async_engine, async_sessionmaker

from db.database_enum import DatabaseEnum


class ConnectionManager:
    _engines = {}

    @staticmethod
    def get_session_factory(database: DatabaseEnum):
        if database not in ConnectionManager._engines:
            engine = create_async_engine(database.value, echo=True)
            ConnectionManager._engines[database] = async_sessionmaker(engine, expire_on_commit=False)

        return ConnectionManager._engines[database]
