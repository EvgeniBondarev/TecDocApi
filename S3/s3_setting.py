class S3Setting:
    def __init__(self, access_key, secret_key, endpoint_url, region_name, bucket_name):
        self.access_key = access_key
        self.secret_key = secret_key
        self.endpoint_url = endpoint_url
        self.region_name = region_name
        self.bucket_name = bucket_name
