using System.Collections.Concurrent;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace ModularMonolith.Shared.Messaging
{
    public class Sender(IServiceProvider serviceProvider) : ISender
    {
        private static readonly ConcurrentDictionary<Type, HandlerInvoker> HandlerCache = new();
        private static readonly ConcurrentDictionary<Type, BehaviorInvoker> BehaviorCache = new();

        public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
        {
            var requestType = request.GetType();

            var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(TResponse));
            var handler = serviceProvider.GetService(handlerType) ?? throw new InvalidOperationException($"No handler registered for {requestType.Name}.");

            var handlerInvoker = HandlerCache.GetOrAdd(requestType, _ =>
            {
                var method = handlerType.GetMethod(nameof(IRequestHandler<,>.Handle))!;
                return new HandlerInvoker(method);
            });

            var behaviorType = typeof(IPipelineBehavior<,>).MakeGenericType(requestType, typeof(TResponse));
            var behaviors = serviceProvider.GetServices(behaviorType).Reverse().ToArray();

            var behaviorInvoker = BehaviorCache.GetOrAdd(requestType, _ =>
            {
                var method = behaviorType.GetMethod(nameof(IPipelineBehavior<,>.Handle))!;
                return new BehaviorInvoker(method);
            });

            RequestHandlerDelegate<TResponse> handlerDelegate = () => (Task<TResponse>)handlerInvoker.Method.Invoke(handler, [request, cancellationToken])!;

            foreach (var behavior in behaviors)
            {
                var next = handlerDelegate;
                handlerDelegate = () => (Task<TResponse>)behaviorInvoker.Method.Invoke(behavior, [request, cancellationToken, next])!;
            }

            return await handlerDelegate();
        }

        private sealed record HandlerInvoker(MethodInfo Method);
        private sealed record BehaviorInvoker(MethodInfo Method);
    }
}