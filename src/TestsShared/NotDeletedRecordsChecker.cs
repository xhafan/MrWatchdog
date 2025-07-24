using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Jobs.Domain;
using MrWatchdog.Core.Features.Users.Domain;
using MrWatchdog.Core.Features.Watchdogs.Domain;

namespace MrWatchdog.TestsShared;

public static class NotDeletedRecordsChecker
{
    private static readonly bool _checkNotDeletedRecords = false;
    private static readonly Lock _lock = new();
    
    public static void CheckNotDeletedRecords(NhibernateUnitOfWork unitOfWork)
    {
        if (!_checkNotDeletedRecords) return;
        
        try
        {
            unitOfWork.BeginTransaction();
        
            if (unitOfWork.Session.QueryOver<Watchdog>().List().Any()
                || unitOfWork.Session.QueryOver<WatchdogAlert>().List().Any()
                || unitOfWork.Session.QueryOver<User>().List().Any()
                || unitOfWork.Session.QueryOver<Job>().List().Any()
               )
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