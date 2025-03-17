from sqlalchemy import Column, Integer, String, DateTime, Boolean
from sqlalchemy.ext.declarative import declarative_base

Base = declarative_base()

class CrTCross(Base):
    __tablename__ = 'cr_t_cross'

    cr_id = Column(Integer, primary_key=True, autoincrement=True)  # IDENTITY(1,1) в MS SQL
    cr_cross = Column(Integer, nullable=False)  # int NOT NULL
    cr_crosscode = Column(String(40), nullable=False)  # nchar(40) NOT NULL
    cr_maincode = Column(String(40), nullable=True)  # nchar(40) NULL
    cr_by = Column(Integer, nullable=False)  # int NOT NULL
    cr_bycode = Column(String(40), nullable=False)  # nchar(40) NOT NULL
    cr_verity = Column(Integer, nullable=False, default=0)  # int DEFAULT 0 NOT NULL
    cr_session_id = Column(Integer, nullable=False)  # int NOT NULL
    cr_deleted = Column(Boolean, nullable=True, default=False)  # bit DEFAULT 0 NULL
    cr_ismainnew = Column(Boolean, nullable=False, default=False)  # bit DEFAULT 0 NOT NULL
    cr_date = Column(DateTime, nullable=True, default='getdate()')  # datetime DEFAULT getdate() NULL