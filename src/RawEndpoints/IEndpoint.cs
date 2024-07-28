namespace RawEndpoints
{
    public interface IEndpoint
    {
        void Define();
    }
    public interface IEndpoint<TRequest> : IEndpoint
    {
    }
    public interface IEndpoint<TRequest, TResponse> : IEndpoint
    {
    }
}
