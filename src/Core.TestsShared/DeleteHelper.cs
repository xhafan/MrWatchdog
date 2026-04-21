using CoreBackend.Account.Features.LoginLink.Domain;
using CoreBackend.Account.Infrastructure.Repositories;
using CoreDdd.Nhibernate.Repositories;
using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Infrastructure.Repositories;

namespace MrWatchdog.Core.TestsShared;

public static class DeleteHelper
{
    extension(NhibernateUnitOfWork unitOfWork)
    {
        public async Task DeleteScraperCascade(Scraper? scraper)
        {
            if (scraper == null) return;

            var scraperRepository = new NhibernateRepository<Scraper>(unitOfWork);
            scraper = await scraperRepository.GetAsync(scraper.Id);
            if (scraper == null) return;

            await scraperRepository.DeleteAsync(scraper);
            await unitOfWork.FlushAsync();

            await unitOfWork.DeleteUserCascade(scraper.User);
        }

        public async Task DeleteWatchdogCascade(Watchdog? watchdog)
        {
            if (watchdog == null) return;

            var watchdogRepository = new NhibernateRepository<Watchdog>(unitOfWork);
            watchdog = await watchdogRepository.GetAsync(watchdog.Id);
            if (watchdog == null) return;

            await watchdogRepository.DeleteAsync(watchdog);
            await unitOfWork.FlushAsync();
        
            await unitOfWork.DeleteScraperCascade(watchdog.Scraper);
            await unitOfWork.DeleteUserCascade(watchdog.User);
        }

        public async Task DeleteUserCascade(User? user)
        {
            if (user == null) return;

            var userRepository = new UserRepository(unitOfWork);
            user = await userRepository.GetAsync(user.Id);
            if (user == null) return;

            await userRepository.DeleteAsync(user);
            await unitOfWork.FlushAsync();
        }

        public async Task DeleteLoginTokenCascade(LoginToken? loginToken)
        {
            if (loginToken == null) return;

            var loginTokenRepository = new LoginTokenRepository(unitOfWork);
            loginToken = await loginTokenRepository.GetAsync(loginToken.Id);
            if (loginToken == null) return;

            await loginTokenRepository.DeleteAsync(loginToken);
            await unitOfWork.FlushAsync();
        }
    }
}