from sqlalchemy import SmallInteger, String, Enum, Integer, Index
from sqlalchemy.testing.schema import Column

from models.base_model import Base


class ArticleOE(Base):
    __tablename__ = "article_oe"

    supplierid = Column(SmallInteger, primary_key=True, nullable=False)
    datasupplierarticlenumber = Column(String, primary_key=True, nullable=False)
    IsAdditive = Column(Enum('True', 'False'), nullable=False)
    OENbr = Column(String, nullable=False)
    manufacturerId = Column(Integer, nullable=False)

    __table_args__ = (
        Index("supplierid_datasupplierarticlenumber", "supplierid", "datasupplierarticlenumber"),
        Index("OENbr", "OENbr"),
        Index("manufacturerId", "manufacturerId"),
        {'mysql_charset': 'utf8', 'mysql_collate': 'utf8_general_ci', 'mysql_row_format': 'COMPRESSED'}
    )