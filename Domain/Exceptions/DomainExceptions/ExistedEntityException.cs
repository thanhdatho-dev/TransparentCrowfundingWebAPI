using System;
using System.Collections.Generic;
using System.Text;

namespace Domain.Exceptions.DomainExceptions
{
    public class ExistedEntityException(string message, string errorCode, object? details = null) 
        : BaseException(message, 409, errorCode, details)
    {
    }
}
