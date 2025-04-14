from sqlalchemy import Column, SmallInteger, String, CHAR
from models.base_model import Base


class SupplierDetails(Base):
    __tablename__ = "supplier_details"

    supplierid = Column(SmallInteger(), primary_key=True, autoincrement=False)
    addresstypeid = Column(CHAR(1), primary_key=True)

    addresstype = Column(String(32), nullable=True)
    city1 = Column(String(64), nullable=True)
    city2 = Column(String(64), nullable=True)
    countrycode = Column(String(64), nullable=True)
    email = Column(String(64), nullable=True)
    fax = Column(String(64), nullable=True)
    homepage = Column(String(64), nullable=True)
    name1 = Column(String(64), nullable=True)
    name2 = Column(String(64), nullable=True)
    postalcodecity = Column(String(32), nullable=True)
    postalcodepob = Column(String(32), nullable=True)
    postalcodewholesaler = Column(String(32), nullable=True)
    postalcountrycode = Column(String(32), nullable=True)
    postofficebox = Column(String(32), nullable=True)
    street1 = Column(String(64), nullable=True)
    street2 = Column(String(64), nullable=True)
    telephone = Column(String(32), nullable=True)