namespace RestaurantSERVICE.DTO.Common
{
    public enum ServiceErrorType
    {
        None = 0,
        Validation = 1,
        NotFound = 2,
        Conflict = 3,
        Forbidden = 4
    }

    /// <summary>
    /// Lets services report "what went wrong" so controllers can pick the right HTTP status
    /// (400 / 404 / 409 / 403) without services knowing anything about HTTP.
    /// </summary>
    public class ServiceResult<T>
    {
        public bool IsSuccess { get; private set; }
        public T? Data { get; private set; }
        public string? Error { get; private set; }
        public ServiceErrorType ErrorType { get; private set; }

        private ServiceResult() { }

        public static ServiceResult<T> Ok(T data)
        {
            return new ServiceResult<T>
            {
                IsSuccess = true,
                Data = data,
                ErrorType = ServiceErrorType.None
            };
        }

        public static ServiceResult<T> Fail(ServiceErrorType errorType, string error)
        {
            return new ServiceResult<T>
            {
                IsSuccess = false,
                ErrorType = errorType,
                Error = error
            };
        }
    }
}
