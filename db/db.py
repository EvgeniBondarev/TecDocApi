from sqlalchemy.ext.asyncio import async_sessionmaker, create_async_engine
from config import *


engine = create_async_engine(TD2018_URL, echo=True)
async_session_maker = async_sessionmaker(engine, expire_on_commit=False)


