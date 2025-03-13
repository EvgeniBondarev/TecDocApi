from sqlalchemy import Column, String, Integer

from models.base_model import Base


class EtPartField(Base):
    __tablename__ = 'et_partfields'

    id = Column(Integer, primary_key=True, nullable=True)
    fieldid = Column(Integer, nullable=True)
    producerid = Column(Integer, nullable=True)
    dataid = Column(String(255), nullable=True)
