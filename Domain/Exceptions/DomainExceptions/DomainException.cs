using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Exceptions.DomainExceptions
{
    public class DomainException(string message, string errorCode, object? details = null) 
        : BaseException(message, 400, errorCode, details)
    {
    }
}
