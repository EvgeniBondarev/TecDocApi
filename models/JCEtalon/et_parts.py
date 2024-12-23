from sqlalchemy import Column, String, Integer, BigInteger, Boolean
from sqlalchemy.types import DECIMAL
from sqlalchemy.dialects.mysql import BIT
from models.base_model import Base

class EtPart(Base):
    __tablename__ = 'et_part'

    id = Column(BigInteger, primary_key=True)
    producerId = Column(Integer, primary_key=True, nullable=False)
    oldId = Column(Integer, primary_key=True, nullable=False)
    code = Column(String(40), nullable=False)
    longcode = Column(String(40), nullable=False)
    weight = Column(DECIMAL(18, 10), nullable=False)
    name = Column(Integer, nullable=False)
    description = Column(Integer, nullable=False)
    V = Column(DECIMAL(18, 10), nullable=False)
    sessionid = Column(Integer, primary_key=True, nullable=False)
    nochangeflag = Column(BIT(1), nullable=False)
    accepted = Column(BIT(1), nullable=False)
    deleted = Column(BIT(1), nullable=False)
    rating = Column(Integer, nullable=False)
    old = Column(BIT(1), nullable=False)
    dead = Column(BIT(1), nullable=False)

    # Index on `code` (optional if used in ORM queries frequently)
    __table_args__ = (
        {'mysql_charset': 'utf8', 'mysql_collate': 'utf8_general_ci'},
    )