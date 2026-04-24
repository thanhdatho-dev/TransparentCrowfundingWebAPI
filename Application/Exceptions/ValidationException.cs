using Domain.Exceptions;

namespace Application.Exceptions
{
    public class ValidationException(IDictionary<string, string[]> errors) 
        : BaseException("One or more validation errors occurred", 422, "VALIDATION_ERROR", errors)
    {
        public IDictionary<string, string[]> Errors { get; } = errors;
    }
}
