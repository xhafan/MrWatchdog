using System.Collections.Concurrent;
using CoreDdd.Queries;

namespace MrWatchdog.TestsShared;

public class FakeQueryHandlerFactory : IQueryHandlerFactory
{
    private readonly ConcurrentDictionary<Type, object> _handlers = new();

    public void RegisterQueryHandler<TQuery>(IQueryHandler<TQuery> queryHandler) where TQuery : IQuery
    {
        _handlers[typeof(TQuery)] = queryHandler;
    }

    public IQueryHandler<TQuery> Create<TQuery>() where TQuery : IQuery
    {
        if (_handlers.TryGetValue(typeof(TQuery), out var handler))
        {
            return (IQueryHandler<TQuery>)handler;
        }
        throw new InvalidOperationException($"No query handler registered for query type {typeof(TQuery).FullName}");
    }

    public void Release<TQuery>(IQueryHandler<TQuery> queryHandler) where TQuery : IQuery
    {
    }
}