from sqlalchemy import Column, String

from models.base_model import Base


class EtProducer(Base):
    __tablename__ = 'et_producers'

    id = Column(String(255),  primary_key=True)
    realid = Column(String(255), nullable=True)
    prefix = Column(String(255), nullable=True)
    name = Column(String(255), nullable=True)
    address = Column(String(255), nullable=True)
    www = Column(String(255), nullable=True)
    rating = Column(String(255), nullable=True)
    existName = Column(String(255), nullable=True)
    existId = Column(String(255), nullable=True)
    domain = Column(String(255), nullable=True)
    tecdocSupplierId = Column(String(255), nullable=True)
    marketPrefix = Column(String(3), nullable=True)

