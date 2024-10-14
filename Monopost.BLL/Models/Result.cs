namespace Monopost.BLL.Models
{
    public class Result
    {
        public bool Success { get; set; }
        public string Message { get; set; }

        public Result(bool success, string message)
        {
            Success = success;
            Message = message;
        }
    }
    public class Result<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T? Data { get; set; }

        public Result(bool success, string message, T? data = default)
        {
            Success = success;
            Message = message;
            Data = data;
        }
    }
}