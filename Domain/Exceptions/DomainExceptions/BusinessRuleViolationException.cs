using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Exceptions.DomainExceptions
{
    public class BusinessRuleViolationException(string message, string errorCode, object? details = null) 
        : BaseException(message, 400, errorCode, details)
    {
    }
}
