using System;

namespace Venther.App.Services.Api;

public class ApiException : Exception
{
    public int StatusCode { get; }

    public ApiException(string message, int statusCode = 0, Exception? innerException = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
    }
}
