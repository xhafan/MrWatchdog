using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Jobs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;

namespace MrWatchdog.TestsShared;

public static class DeleteHelper
{
    public static async Task DeleteJobCascade(this NhibernateUnitOfWork unitOfWork, Guid jobGuid)
    {
        var jobRepository = new JobRepository(unitOfWork);
        var job = await jobRepository.GetByGuidAsync(jobGuid);
        await unitOfWork.DeleteJobCascade(job);
    }

    public static async Task DeleteJobCascade(this NhibernateUnitOfWork unitOfWork, Job? job)
    {
        if (job == null) return;

        var jobRepository = new JobRepository(unitOfWork);
        job = await jobRepository.GetAsync(job.Id);
        if (job == null) return;

        await jobRepository.DeleteAsync(job);
    }

    public static async Task DeleteWatchdogCascade(this NhibernateUnitOfWork unitOfWork, Watchdog? watchdog)
    {
        if (watchdog == null) return;

        var watchdogRepository = new NhibernateRepository<Watchdog>(unitOfWork);
        watchdog = await watchdogRepository.GetAsync(watchdog.Id);
        if (watchdog == null) return;

        await watchdogRepository.DeleteAsync(watchdog);

        await unitOfWork.DeleteUserCascade(watchdog.User);
    }

    public static async Task DeleteWatchdogAlertCascade(this NhibernateUnitOfWork unitOfWork, WatchdogAlert? watchdogAlert)
    {
        if (watchdogAlert == null) return;

        var watchdogAlertRepository = new NhibernateRepository<WatchdogAlert>(unitOfWork);
        watchdogAlert = await watchdogAlertRepository.GetAsync(watchdogAlert.Id);
        if (watchdogAlert == null) return;

        await watchdogAlertRepository.DeleteAsync(watchdogAlert);
        await unitOfWork.DeleteWatchdogCascade(watchdogAlert.Watchdog);
        await unitOfWork.DeleteUserCascade(watchdogAlert.User);
    }

    public static async Task DeleteUserCascade(this NhibernateUnitOfWork unitOfWork, User? user)
    {
        if (user == null) return;

        var userRepository = new UserRepository(unitOfWork);
        user = await userRepository.GetAsync(user.Id);
        if (user == null) return;

        await userRepository.DeleteAsync(user);
    }

    public static async Task DeleteLoginTokenCascade(this NhibernateUnitOfWork unitOfWork, LoginToken? loginToken)
    {
        if (loginToken == null) return;

        var loginTokenRepository = new LoginTokenRepository(unitOfWork);
        loginToken = await loginTokenRepository.GetAsync(loginToken.Id);
        if (loginToken == null) return;

        await loginTokenRepository.DeleteAsync(loginToken);
    }
}