using CoreDdd.Queries;

namespace MrWatchdog.TestsShared;

public class FakeQueryHandlerFactory<TQueryHandlerQuery> : IQueryHandlerFactory
    where TQueryHandlerQuery : IQuery
{
    private readonly IQueryHandler<TQueryHandlerQuery> _queryHandler;

    public FakeQueryHandlerFactory(IQueryHandler<TQueryHandlerQuery> queryHandler)
    {
        _queryHandler = queryHandler;
    }

    public IQueryHandler<TQuery> Create<TQuery>() where TQuery : IQuery
    {
        return (IQueryHandler<TQuery>)_queryHandler;
    }

    public void Release<TQuery>(IQueryHandler<TQuery> queryHandler) where TQuery : IQuery
    {
    }
}