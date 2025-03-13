from abc import ABC, abstractmethod


class AbstractReadRepository(ABC):
    @abstractmethod
    async def find_all(self):
        """Получить все записи."""
        raise NotImplementedError

    @abstractmethod
    async def find(self, filter_condition=None):
        """Получить одну запись по условию."""
        raise NotImplementedError

    @abstractmethod
    async def find_one(self, filter_condition=None):
        """Получить одну запись по условию."""
        raise NotImplementedError