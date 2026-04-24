using Domain.Exceptions;

namespace Application.Exceptions
{
    public class UnauthorizedException(string message, string errorCode, object? details = null)
        : BaseException(message, 401, errorCode, details)
    {
    }
}
