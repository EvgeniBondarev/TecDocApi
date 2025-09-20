namespace OzonApiManage
{
    public class OzonApiResponse<T>
    {
        public bool IsSuccess { get; set; }
        public T Result { get; set; }
        public string ErrorMessage { get; set; }

        public OzonApiResponse(bool isSuccess, T result, string errorMessage)
        {
            IsSuccess = isSuccess;
            Result = result;
            ErrorMessage = errorMessage;
        }

        public OzonApiResponse()
        {
            IsSuccess = false;
            Result = default(T);
            ErrorMessage = string.Empty;
        }
    }
}
