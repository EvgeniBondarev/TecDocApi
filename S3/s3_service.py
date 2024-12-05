from boto3.session import Session

from S3.s3_setting import S3Setting


class S3Service:
    def __init__(self, s3_setting: S3Setting):
        self.s3_setting = s3_setting
        self.session = Session(
            aws_access_key_id=self.s3_setting.access_key,
            aws_secret_access_key=self.s3_setting.secret_key
        )
        self.s3_client = self.session.client(
            's3',
            endpoint_url=self.s3_setting.endpoint_url,
            region_name=self.s3_setting.region_name
        )

    def get_image_url(self, file_name: str, expires_in: int = 86400):
        try:
            url = self.s3_client.generate_presigned_url(
                'get_object',
                Params={'Bucket': self.s3_setting.bucket_name, 'Key': file_name},
                ExpiresIn=expires_in,
                HttpMethod='GET'
            )
            return url
        except Exception as e:
            print(f"Error when generating link: {str(e)}")
            return None
