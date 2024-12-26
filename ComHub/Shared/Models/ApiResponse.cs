namespace ComHub.Shared.Models;

public class Response
{
    public int StatusCode { get; set; }
    public required string Message { get; set; }
}

public class ErrorResponse : Response
{
    public required string Error { get; set; }
}

public class DataResponse<T> : Response
{
    public required T Data { get; set; }
}
