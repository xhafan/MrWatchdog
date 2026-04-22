using System.Collections.Concurrent;
using CoreDdd.Queries;

namespace CoreBackend.TestsShared;

public class FakeQueryHandlerFactory : IQueryHandlerFactory
{
    private readonly ConcurrentDictionary<Type, object> _handlers = new();

    public void RegisterQueryHandler<TQuery, TResult>(IQueryHandler<TQuery, TResult> queryHandler) where TQuery : IQuery<TResult>
    {
        _handlers[typeof(TQuery)] = queryHandler;
    }

    public IQueryHandler<TQuery, TResult> Create<TQuery, TResult>() where TQuery : IQuery<TResult>
    {
        if (_handlers.TryGetValue(typeof(TQuery), out var handler))
        {
            return (IQueryHandler<TQuery, TResult>)handler;
        }
        throw new InvalidOperationException($"No query handler registered for query type {typeof(TQuery).FullName}");
    }

    public void Release<TQuery, TResult>(IQueryHandler<TQuery, TResult> queryHandler) where TQuery : IQuery<TResult>
    {
    }
}