using CoreDdd.Nhibernate.UnitOfWorks;
using CoreUtils;
using NHibernate.Criterion;

namespace CoreBackend.TestsShared;

public static class NotDeletedRecordsChecker
{
    private static readonly bool _checkNotDeletedRecords = false;
    private static readonly Lock _lock = new();

    public static void CheckNotDeletedRecords(NhibernateUnitOfWork unitOfWork, ICollection<Type>? aggregateRootEntityTypes)
    {
        if (!_checkNotDeletedRecords) return;

        Guard.Hope(aggregateRootEntityTypes != null && aggregateRootEntityTypes.Any(), "No aggregate root entity types specified.");

        try
        {
            unitOfWork.BeginTransaction();

            var hasNotDeletedRecords = aggregateRootEntityTypes.Any(type =>
                unitOfWork.Session!.CreateCriteria(type)
                    .SetProjection(Projections.RowCount())
                    .UniqueResult<int>() > 0
            );

            if (hasNotDeletedRecords)
            {
                lock (_lock)
                {
                    File.AppendAllText(
                        @"C:\Projects\MrWatchdog\src\not_deleted_records.log",
                        TestContext.CurrentContext.Test.FullName + Environment.NewLine
                    );
                }
            }
        }
        finally
        {
            unitOfWork.Rollback();
        }
    }
}