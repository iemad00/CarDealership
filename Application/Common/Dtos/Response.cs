namespace CarDealership.Application.Common.Dtos;

public class Response
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
}

public class Response<T> : Response
{
    public T? Data { get; set; }
}


