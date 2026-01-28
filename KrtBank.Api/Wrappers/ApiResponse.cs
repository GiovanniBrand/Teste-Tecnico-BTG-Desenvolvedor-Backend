namespace KrtBank.Api.Wrappers
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }

        public ApiResponse(T data, string message = "Operação realizada com sucesso.")
        {
            Success = true;
            Message = message;
            Data = data;
        }

        public static ApiResponse<T> Create(T data, string message = "Sucesso") => new(data, message);
    }

    public class ApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }

        public ApiResponse(bool success, string message)
        {
            Success = success;
            Message = message;
        }

        public static ApiResponse SuccessResponse(string message = "Sucesso") => new(true, message);
    }
}
