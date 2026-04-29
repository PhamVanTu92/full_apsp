using System.Net;

namespace BackEndAPI.Infr.Exceptions;

public class BusinessException(string message, HttpStatusCode statusCode, string errorCode) : Exception(message)
{
    public string ErrorCode { get; } = errorCode;
    public HttpStatusCode StatusCode { get; } = statusCode;
}