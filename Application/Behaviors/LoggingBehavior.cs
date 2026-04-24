using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Application.Behaviors
{
    public class LoggingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    {
        private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

        public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
            => _logger = logger;

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken ct)
        {
            var name = typeof(TRequest).Name;
            _logger.LogInformation("Handling {RequestName}: {@Request}", name, request);

            var sw = Stopwatch.StartNew();
            var response = await next(ct);
            sw.Stop();

            if (sw.ElapsedMilliseconds > 500)
                _logger.LogWarning("Slow request {RequestName}: {ElapsedMs}ms", name, sw.ElapsedMilliseconds);

            return response;
        }
    }
}
