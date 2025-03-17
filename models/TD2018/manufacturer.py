from sqlalchemy import Column, Integer, String, Enum, Index
from sqlalchemy.ext.declarative import declarative_base

Base = declarative_base()

class Manufacturer(Base):
    __tablename__ = "manufacturers"

    id = Column(Integer, primary_key=True, nullable=False)
    canbedisplayed = Column(Enum('True', 'False'), nullable=True)
    description = Column(String(64), nullable=True)
    fulldescription = Column(String(64), nullable=True)
    haslink = Column(Enum('True', 'False'), nullable=True)
    isaxle = Column(Enum('True', 'False'), primary_key=True, nullable=False)
    iscommercialvehicle = Column(Enum('True', 'False'), primary_key=True, nullable=False)
    isengine = Column(Enum('True', 'False'), primary_key=True, nullable=False)
    ismotorbike = Column(Enum('True', 'False'), primary_key=True, nullable=False)
    ispassengercar = Column(Enum('True', 'False'), primary_key=True, nullable=False)
    istransporter = Column(Enum('True', 'False'), primary_key=True, nullable=False)
    isvgl = Column(Enum('True', 'False'), nullable=True)
    matchcode = Column(String(64), nullable=True)

    __table_args__ = (
        Index("canbedisplayed", "canbedisplayed"),
        {'mysql_charset': 'utf8', 'mysql_collate': 'utf8_general_ci', 'mysql_row_format': 'COMPRESSED'}
    )