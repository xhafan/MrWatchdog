using CoreDdd.Domain;
using MrWatchdog.Core.Features.Jobs.Domain;
using MrWatchdog.Core.Infrastructure.DataProtections;
using MrWatchdog.Core.Infrastructure.Rebus;
using NHibernate;
using NHibernate.Type;

namespace MrWatchdog.Core.Infrastructure.Interceptors;

public class EntityJobTrackingInterceptor : EmptyInterceptor
{
    public override bool OnSave(
        object entity,
        object id,
        object[] state,
        string[] propertyNames,
        IType[] types
        )
    {
        if (_IsCreatedEntityToTrack(entity))
        {
            _AddAffectedEntity(entity, (long)id, isCreated: true);
        }
        return base.OnSave(entity, id, state, propertyNames, types);
    }
    
    private bool _IsCreatedEntityToTrack(object entity)
    {
        return entity is not Job 
               && entity is not JobAffectedEntity
               && entity is not JobHandlingAttempt
               && entity is not DataProtectionKey;
    }    

    public override bool OnLoad(
        object entity,
        object id,
        object[] state,
        string[] propertyNames,
        IType[] types
        )
    {
        if (_IsLoadedAggregateRootEntityToTrack(entity))
        {
            _AddAffectedEntity(entity, (long)id, isCreated: false);
        }
        return base.OnLoad(entity, id, state, propertyNames, types);
    }

    private bool _IsLoadedAggregateRootEntityToTrack(object entity)
    {
        return entity is not Job
               && entity is not DataProtectionKey
               && entity is IAggregateRoot;
    }    

    private void _AddAffectedEntity(object entity, long id, bool isCreated)
    {
        if (JobContext.AffectedEntities.Value != null)
        {
            JobContext.AffectedEntities.Value.Add((entity.GetType().Name, id, isCreated));
        }
    }
}