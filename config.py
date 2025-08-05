import os
from dotenv import load_dotenv

load_dotenv()

DB_HOST = os.getenv("DB_HOST")
DB_PORT = os.getenv("DB_PORT", "3306")
DB_USER = os.getenv("DB_USER")
DB_PASS = os.getenv("DB_PASS")



TD2018_URL = f"mysql+aiomysql://{DB_USER}:{DB_PASS}@{DB_HOST}:{DB_PORT}/TD2018?charset=utf8mb4"
JCEtalon_URL = f"mysql+aiomysql://{DB_USER}:{DB_PASS}@{DB_HOST}:{DB_PORT}/JCEtalon?charset=utf8mb4"
MNK_URL = f"mysql+aiomysql://{DB_USER}:{DB_PASS}@{DB_HOST}:{DB_PORT}/MNK?charset=utf8mb4"


DB_MS_HOST = os.getenv("DB_MS_HOST")
DB_MS_PORT = os.getenv("DB_MS_PORT", "1433")
DB_MS_USER = os.getenv("DB_MS_USER")
DB_MS_PASS = os.getenv("DB_MS_PASS")

JCCross_URL = f"mssql+aioodbc://{DB_MS_USER}:{DB_MS_PASS}@{DB_MS_HOST}:{DB_MS_PORT}/JCCross?driver=ODBC+Driver+17+for+SQL+Server"

S3_ACCESS_KEY = os.getenv("S3_ACCESS_KEY")
S3_SECRET_KEY = os.getenv("S3_SECRET_KEY")
S3_ENDPOINT_URL = os.getenv("S3_ENDPOINT_URL")
S3_REGION_NAME = os.getenv("S3_REGION_NAME")
S3_BUCKET_NAME = os.getenv("S3_BUCKET_NAME")

ABCP_USER_LOGIN = os.getenv("ABCP_USER_LOGIN")
ABCP_USER_PSW = os.getenv("ABCP_USER_PSW")
