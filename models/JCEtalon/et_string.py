from sqlalchemy import Column, BigInteger, Integer, String
from models.base_model import Base


class EtString(Base):
    __tablename__ = 'et_string'

    id = Column(BigInteger, primary_key=True, autoincrement=True)
    producerId = Column(Integer, nullable=True)
    oldId = Column(Integer, nullable=True)
    idstr = Column(Integer, nullable=True)
    lng = Column(Integer, nullable=True)
    text = Column(String(4000), nullable=True)
