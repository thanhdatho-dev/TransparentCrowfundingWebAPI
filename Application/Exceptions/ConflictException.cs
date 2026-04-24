using Domain.Exceptions;

namespace Application.Exceptions
{
    public class ConflictException(string message, string errorCode, object? details = null) 
        : BaseException(message, 409, errorCode, details)
    {
    }
}
