using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Exceptions.DomainExceptions
{
    public class EntityNotFoundException(string message, string errorCode, object? details = null) 
        : BaseException(message, 404, errorCode, details)
    {
    }
}
