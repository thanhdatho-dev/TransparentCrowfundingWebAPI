namespace Domain.Exceptions
{
    public class BaseException : Exception
    {
        public int StatusCode { get; }
        public string ErrorCode { get; }
        public object? Details { get; }

        protected BaseException(
            string message,
            int statusCode,
            string errorCode,
            object? details = null) : base(message)
        {
            StatusCode = statusCode;
            ErrorCode = errorCode;
            Details = details;
        }
    }
}
