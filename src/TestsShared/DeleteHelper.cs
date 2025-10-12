﻿using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Jobs.Domain;
using MrWatchdog.Core.Features.Jobs.Services;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;

namespace MrWatchdog.TestsShared;

public static class DeleteHelper
{
    public static async Task DeleteJobCascade(this NhibernateUnitOfWork unitOfWork, Guid jobGuid, bool waitForJobCompletion = false)
    {
        var jobRepository = new JobRepository(unitOfWork);
        var job = await jobRepository.GetByGuidAsync(jobGuid);
        await unitOfWork.DeleteJobCascade(job, waitForJobCompletion);
    }

    public static async Task DeleteJobCascade(this NhibernateUnitOfWork unitOfWork, Job? job, bool waitForJobCompletion = false)
    {
        if (job == null) return;

        var jobRepository = new JobRepository(unitOfWork);
        job = await jobRepository.GetAsync(job.Id);
        if (job == null) return;

        if (waitForJobCompletion)
        {
            var jobCompletionAwaiter = new JobCompletionAwaiter(TestFixtureContext.NhibernateConfigurator);
            await jobCompletionAwaiter.WaitForJobCompletion(job.Guid);
        
            await unitOfWork.Session!.EvictAsync(job);
            job = await jobRepository.LoadByIdAsync(job.Id);
        }

        await jobRepository.DeleteAsync(job);
        await unitOfWork.FlushAsync();
    }

    public static async Task DeleteWatchdogCascade(this NhibernateUnitOfWork unitOfWork, Watchdog? watchdog)
    {
        if (watchdog == null) return;

        var watchdogRepository = new NhibernateRepository<Watchdog>(unitOfWork);
        watchdog = await watchdogRepository.GetAsync(watchdog.Id);
        if (watchdog == null) return;

        await watchdogRepository.DeleteAsync(watchdog);
        await unitOfWork.FlushAsync();

        await unitOfWork.DeleteUserCascade(watchdog.User);
    }

    public static async Task DeleteWatchdogSearchCascade(this NhibernateUnitOfWork unitOfWork, WatchdogSearch? watchdogSearch)
    {
        if (watchdogSearch == null) return;

        var watchdogSearchRepository = new NhibernateRepository<WatchdogSearch>(unitOfWork);
        watchdogSearch = await watchdogSearchRepository.GetAsync(watchdogSearch.Id);
        if (watchdogSearch == null) return;

        await watchdogSearchRepository.DeleteAsync(watchdogSearch);
        await unitOfWork.FlushAsync();
        
        await unitOfWork.DeleteWatchdogCascade(watchdogSearch.Watchdog);
        await unitOfWork.DeleteUserCascade(watchdogSearch.User);
    }

    public static async Task DeleteUserCascade(this NhibernateUnitOfWork unitOfWork, User? user)
    {
        if (user == null) return;

        var userRepository = new UserRepository(unitOfWork);
        user = await userRepository.GetAsync(user.Id);
        if (user == null) return;

        await userRepository.DeleteAsync(user);
        await unitOfWork.FlushAsync();
    }

    public static async Task DeleteLoginTokenCascade(this NhibernateUnitOfWork unitOfWork, LoginToken? loginToken)
    {
        if (loginToken == null) return;

        var loginTokenRepository = new LoginTokenRepository(unitOfWork);
        loginToken = await loginTokenRepository.GetAsync(loginToken.Id);
        if (loginToken == null) return;

        await loginTokenRepository.DeleteAsync(loginToken);
        await unitOfWork.FlushAsync();
    }
}