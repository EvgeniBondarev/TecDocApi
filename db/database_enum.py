from enum import Enum

from config import TD2018_URL, JCEtalon_URL, MNK_URL, JCCross_URL


class DatabaseEnum(Enum):
    TD2018 = TD2018_URL
    JCEtalon = JCEtalon_URL
    MNK = MNK_URL
    JCCross = JCCross_URL
