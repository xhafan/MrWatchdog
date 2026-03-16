using System.Collections;
using NHibernate.Type;
using NHibernate;
using NHibernate.SqlCommand;

namespace MrWatchdog.Core.Infrastructure.Interceptors;

// taken from https://stackoverflow.com/a/33289268/379279 and modified for nullable 
public class CompositeInterceptor(IEnumerable<IInterceptor> interceptors) : IInterceptor
{
    private readonly IEnumerable<IInterceptor> _interceptors = interceptors.ToList();

    public void AfterTransactionBegin(ITransaction tx)
    {
        foreach (var interceptor in _interceptors)
        {
            interceptor.AfterTransactionBegin(tx);
        }
    }

    public void AfterTransactionCompletion(ITransaction tx)
    {
        foreach (var interceptor in _interceptors)
        {
            interceptor.AfterTransactionCompletion(tx);
        }
    }

    public void BeforeTransactionCompletion(ITransaction tx)
    {
        foreach (var interceptor in _interceptors)
        {
            interceptor.BeforeTransactionCompletion(tx);
        }
    }

    public int[]? FindDirty(
        object entity, 
        object id, 
        object[] currentState, 
        object[] previousState, 
        string[] propertyNames, 
        IType[] types
    )
    {
        var results = _interceptors
            .Select(interceptor => interceptor.FindDirty(entity, id, currentState, previousState, propertyNames, types))
            .Where(result => result != null)
            .SelectMany(x => x)
            .Distinct()
            .ToArray();
        return !results.Any() ? null : results;
    }

    public object? GetEntity(string entityName, object id)
    {
        return _interceptors
            .Select(interceptor => interceptor.GetEntity(entityName, id))
            .FirstOrDefault(result => result != null);
    }

    public string? GetEntityName(object entity)
    {
        return _interceptors
            .Select(interceptor => interceptor.GetEntityName(entity))
            .FirstOrDefault(result => result != null);
    }

    public object? Instantiate(string entityName, object id)
    {
        return _interceptors
            .Select(interceptor => interceptor.Instantiate(entityName, id))
            .FirstOrDefault(result => result != null);
    }

    public bool? IsTransient(object entity)
    {
        return _interceptors
            .Select(interceptor => interceptor.IsTransient(entity))
            .FirstOrDefault(result => result != null);
    }

    public void OnCollectionRecreate(object collection, object key)
    {
        foreach (var interceptor in _interceptors)
        {
            interceptor.OnCollectionRecreate(collection, key);
        }
    }

    public void OnCollectionRemove(object collection, object key)
    {
        foreach (var interceptor in _interceptors)
        {
            interceptor.OnCollectionRemove(collection, key);
        }
    }

    public void OnCollectionUpdate(object collection, object key)
    {
        foreach (var interceptor in _interceptors)
        {
            interceptor.OnCollectionUpdate(collection, key);
        }
    }

    public void OnDelete(
        object entity, 
        object id, 
        object[] state, 
        string[] propertyNames, 
        IType[] types
    )
    {
        foreach (var interceptor in _interceptors)
        {
            interceptor.OnDelete(entity, id, state, propertyNames, types);
        }
    }

    public bool OnFlushDirty(
        object entity, 
        object id, 
        object[] currentState, 
        object[] previousState, 
        string[] propertyNames, 
        IType[] types
    )
    {
        return _interceptors.Any(interceptor => interceptor.OnFlushDirty(entity, id, currentState, previousState, propertyNames, types));
    }

    public bool OnLoad(
        object entity, 
        object id, 
        object[] state, 
        string[] propertyNames,
        IType[] types
    )
    {
        return _interceptors.Any(interceptor => interceptor.OnLoad(entity, id, state, propertyNames, types));
    }

    public SqlString OnPrepareStatement(SqlString sql)
    {
        return _interceptors.Aggregate(sql, (current, interceptor) => interceptor.OnPrepareStatement(current));
    }

    public bool OnSave(
        object entity, 
        object id, 
        object[] state, 
        string[] propertyNames, 
        IType[] types
    )
    {
        return _interceptors.Any(interceptor => interceptor.OnSave(entity, id, state, propertyNames, types));
    }

    public void PostFlush(ICollection entities)
    {
        foreach (var interceptor in _interceptors)
        {
            interceptor.PostFlush(entities);
        }
    }

    public void PreFlush(ICollection entities)
    {
        foreach (var interceptor in _interceptors)
        {
            interceptor.PreFlush(entities);
        }
    }

    public void SetSession(ISession session)
    {
        foreach (var interceptor in _interceptors)
        {
            interceptor.SetSession(session);
        }
    }
}