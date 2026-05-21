namespace FantasyPitchXI.DTO
{
    public class ApiResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }

        public static ApiResponse Pass(string message)
        {
            return new ApiResponse
            {
                Success = true,
                Message = message
            };
        }

        public static ApiResponse Fail(string message)
        {
            return new ApiResponse
            {
                Success = false,
                Message = message
            };
        }

    }

    public class ApiResponse<T> : ApiResponse
    {
        public T Data { get; set; }

        public static ApiResponse<T> Pass(string message, T data)
        {
            return new ApiResponse<T>
            {
                Success = true,
                Message = message,
                Data = data
            };
        }

        public static ApiResponse<T> Fail(string message)
        {
            return new ApiResponse<T>
            {
                Success = false,
                Message = message,
            };
        }
    }

}
