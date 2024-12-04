from sqlalchemy import select
from sqlalchemy.orm import DeclarativeMeta

from db.connection_manager import ConnectionManager
from db.database_enum import DatabaseEnum
from db.db import async_session_maker

from repositories.ABC.abstract_repository import AbstractReadRepository


class SQLAlchemyReadRepository(AbstractReadRepository):
    model: DeclarativeMeta = None
    join_models: list[DeclarativeMeta] = None
    database: DatabaseEnum = None

    def __init__(self):
        if not self.database:
            raise ValueError("You must pass data resources in the 'database' class attribute.")
        self.session_factory = ConnectionManager.get_session_factory(self.database)

    async def find_all(self):
        """Получить все записи модели с опциональным объединением."""
        async with self.session_factory() as session:
            query = select(self.model) if not self.join_models else select(self.model, *self.join_models)
            if self.join_models:
                for join_model in self.join_models:
                    query = query.join(join_model)

            result = await session.execute(query)
            return result.scalars().all()

    async def find(self, filter_condition=None):
        """Получить записи модели по условию."""
        async with self.session_factory() as session:
            query = select(self.model)
            if self.join_models:
                for join_model in self.join_models:
                    query = query.join(join_model)

            if filter_condition is not None:
                query = query.where(filter_condition)

            result = await session.execute(query)
            return result.scalars().all()

    async def find_one(self, filter_condition=None):
        """Получить одну запись модели по условию."""
        async with self.session_factory() as session:
            query = select(self.model)
            if self.join_models:
                for join_model in self.join_models:
                    query = query.join(join_model)

            if filter_condition is not None:
                query = query.where(filter_condition)

            result = await session.execute(query)
            return result.scalars().first()