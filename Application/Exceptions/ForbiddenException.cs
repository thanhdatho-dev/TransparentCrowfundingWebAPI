using Domain.Exceptions;

namespace Application.Exceptions
{
    public class ForbiddenException(string message, string errorCode, object? details = null)
        : BaseException(message, 403, errorCode, details)
    {
    }
}
