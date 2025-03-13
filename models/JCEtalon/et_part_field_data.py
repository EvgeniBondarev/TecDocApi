from sqlalchemy import Column, Integer, String
from models.base_model import Base


class EtPartFieldData(Base):
    __tablename__ = 'et_partfieldsdata'

    id = Column(Integer, primary_key=True, autoincrement=True)
    data = Column(String(255), nullable=True)
