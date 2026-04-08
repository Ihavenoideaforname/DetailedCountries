namespace DetailedCountries.Server.Models.BackendModels
{
    public class APIResult<T>
    {
        public bool Success { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }

        public static APIResult<T> Ok(T data, int statusCode = 200) =>
            new APIResult<T> { Success = true, StatusCode = statusCode, Data = data };

        public static APIResult<T> Fail(string message, int statusCode) =>
            new APIResult<T> { Success = false, StatusCode = statusCode, Message = message };
    }
}
