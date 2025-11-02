namespace ComedyPull.Application.Http
{
    public interface IHandler<in TRequest, TResponse>
    {
        public Task<TResponse> HandleAsync(TRequest request, CancellationToken stoppingToken);
    }
    
    public interface IHandler<in TRequest>
    {
        public Task HandleAsync(TRequest request, CancellationToken stoppingToken);
    }
}