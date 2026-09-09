using ModularMonolith.Shared.Common;
using ModularMonolith.Shared.Interfaces;
using ModularMonolith.Shared.Messaging;
using FluentValidation;

namespace ModularMonolith.Shared.Behaviors
{
    public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
        : IPipelineBehavior<TRequest, TResponse>
        where TRequest : IRequest<TResponse>
        where TResponse : IResult
    {
        public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
        {
            var validatorList = validators.ToList();
            if (validatorList.Count == 0)
            {
                return await next();
            }

            var failures = new List<string>();

            foreach (var validator in validatorList)
            {
                var result = await validator.ValidateAsync(request, cancellationToken);

                if (!result.IsValid)
                {
                    failures.AddRange(result.Errors.Select(x =>
                        string.IsNullOrWhiteSpace(x.PropertyName)
                            ? x.ErrorMessage
                            : $"{x.PropertyName}: {x.ErrorMessage}"));
                }
            }

            if (failures.Count == 0)
            {
                return await next();
            }

            return (TResponse)TResponse.Failure(ErrorType.Validation, "One or more validation errors occurred.", failures);
        }
    }
}