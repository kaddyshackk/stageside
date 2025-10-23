namespace ComedyPull.Application.Interfaces
{
    public interface IHandler<TRequest, TResponse> where TRequest : class where TResponse : class
    {
        public Task<TResponse?> HandleAsync(TRequest request, CancellationToken cancellationToken);
    }
}