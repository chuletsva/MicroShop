﻿using FluentValidation;
using MediatR;
using Microsoft.Extensions.Logging;
using Ordering.Application.Exceptions;

namespace Ordering.Application.PipelineBehaviours;

public class ValidationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger _logger;
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehaviour(
        ILogger<ValidationBehaviour<TRequest, TResponse>> logger,
        IEnumerable<IValidator<TRequest>> validators)
    {
        _logger = logger;
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
    {
        if (_validators.Any())
        {
            try
            {
                var validation = _validators.Select(x => x.ValidateAsync(request, cancellationToken));

                var errors = (await Task.WhenAll(validation))
                    .Where(x => x.Errors is not null)
                    .SelectMany(x => x.Errors)
                    .ToArray();

                if (errors.Length > 0)
                    throw new RequestValidationException(errors);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occured while request validation");

                throw;
            }
        }

        return await next();
    }
}
