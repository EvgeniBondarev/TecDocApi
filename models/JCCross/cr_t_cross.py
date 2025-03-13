from sqlalchemy import Column, String
from models.base_model import Base

class CrTCross(Base):
    __tablename__ = 'cr_t_cross'

    cr_id = Column(String(255), primary_key=True)
    cr_cross = Column(String(255))
    cr_crosscode = Column(String(255))
    cr_maincode = Column(String(255))
    cr_by = Column(String(255))
    cr_bycode = Column(String(255))
    cr_verity = Column(String(255))
    cr_session_id = Column(String(255))
    cr_deleted = Column(String(255))
    cr_ismainnew = Column(String(255))
    cr_date = Column(String(255))

    __table_args__ = (
        {'mysql_charset': 'utf8', 'mysql_collate': 'utf8_general_ci'},
    )
