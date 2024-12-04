from sqlalchemy import Column, SmallInteger, String, Integer, Enum

from models.base_model import Base


class Suppliers(Base):
    __tablename__ = "suppliers"

    id = Column(SmallInteger(), primary_key=True, autoincrement=False)

    dataversion = Column(SmallInteger(), nullable=True)
    description = Column(String(32), nullable=True, index=True)
    matchcode = Column(String(32), nullable=True)
    nbrofarticles = Column(Integer(), nullable=True)
    hasnewversionarticles = Column(Enum("True", "False"), nullable=True)
