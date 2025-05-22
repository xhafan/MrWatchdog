using CoreDdd.Domain;
using MrWatchdog.Core.Features.Jobs.Domain;
using NHibernate.Type;
using NHibernate;
using MrWatchdog.Core.Rebus;

namespace MrWatchdog.Core.Infrastructure.Interceptors;

public class AggregateRootEntityJobTrackingInterceptor : EmptyInterceptor
{
    public override bool OnSave(
        object entity,
        object id,
        object[] state,
        string[] propertyNames,
        IType[] types
        )
    {
        if (_IsEntityAggregateRoot(entity))
        {
            _AddAffectedAggregateRootEntity(entity, (long)id);
        }
        return base.OnSave(entity, id, state, propertyNames, types);
    }

    public override bool OnLoad(
        object entity,
        object id,
        object[] state,
        string[] propertyNames,
        IType[] types
        )
    {
        if (_IsEntityAggregateRoot(entity))
        {
            _AddAffectedAggregateRootEntity(entity, (long)id);
        }
        return base.OnLoad(entity, id, state, propertyNames, types);
    }

    private bool _IsEntityAggregateRoot(object entity)
    {
        return entity is not Job
               && entity is IAggregateRoot;
    }

    private void _AddAffectedAggregateRootEntity(object entity, long id)
    {
        if (JobTrackingIncomingStep.AffectedAggregateRootEntities.Value != null)
        {
            JobTrackingIncomingStep.AffectedAggregateRootEntities.Value.Add((entity.GetType().Name, id));
        }
    }
}