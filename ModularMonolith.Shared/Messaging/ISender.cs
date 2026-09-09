namespace ModularMonolith.Shared.Messaging
{
    public interface ISender
    {
        Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
    }
}
