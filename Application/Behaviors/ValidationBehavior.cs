using FluentValidation;
using MediatR;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Behaviors
{
    public class ValidationBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    {
        private readonly IEnumerable<IValidator<TRequest>> _validators;

        public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
            => _validators = validators;

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken ct)
        {
            if (!_validators.Any()) return await next(ct);

            var failures = _validators
                .Select(v => v.Validate(new ValidationContext<TRequest>(request)))
                .SelectMany(r => r.Errors)
                .Where(e => e is not null)
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray());

            if (failures.Count > 0)
                throw new ValidationException(
                    System.Text.Json.JsonSerializer.Serialize(failures));

            return await next(ct);
        }
    }
}
