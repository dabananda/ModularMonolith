using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;

namespace ModularMonolith.Shared.Messaging
{
    public class Sender(IServiceProvider serviceProvider) : ISender
    {
        private static readonly ConcurrentDictionary<Type, object> HandlerWrappers = new();

        public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(request);

            var requestType = request.GetType();

            var wrapper = (RequestHandlerWrapper<TResponse>)HandlerWrappers.GetOrAdd(
                requestType,
                static reqType =>
                {
                    var wrapperType = typeof(RequestHandlerWrapperImpl<,>).MakeGenericType(reqType, typeof(TResponse));
                    return Activator.CreateInstance(wrapperType)!;
                });

            return wrapper.Handle(request, serviceProvider, cancellationToken);
        }

        private abstract class RequestHandlerWrapper<TResponse>
        {
            public abstract Task<TResponse> Handle(
                IRequest<TResponse> request,
                IServiceProvider provider,
                CancellationToken cancellationToken);
        }

        private sealed class RequestHandlerWrapperImpl<TRequest, TResponse> : RequestHandlerWrapper<TResponse>
            where TRequest : IRequest<TResponse>
        {
            public override async Task<TResponse> Handle(
                IRequest<TResponse> request,
                IServiceProvider provider,
                CancellationToken cancellationToken)
            {
                var handler = provider.GetService<IRequestHandler<TRequest, TResponse>>()
                    ?? throw new InvalidOperationException($"No handler registered for {typeof(TRequest).Name}.");

                var behaviors = provider.GetServices<IPipelineBehavior<TRequest, TResponse>>().Reverse();

                RequestHandlerDelegate<TResponse> current = () => handler.Handle((TRequest)request, cancellationToken);

                foreach (var behavior in behaviors)
                {
                    var next = current;
                    current = () => behavior.Handle((TRequest)request, cancellationToken, next);
                }

                return await current();
            }
        }
    }
}