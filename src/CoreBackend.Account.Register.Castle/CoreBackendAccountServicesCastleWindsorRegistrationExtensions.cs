using Castle.MicroKernel.Registration;
using Castle.Windsor;
using CoreBackend.Account.Features.Account.Commands;
using CoreBackend.Account.Features.Account.Queries;
using CoreBackend.Account.Infrastructure.Repositories;
using CoreDdd.Domain.Repositories;
using CoreDdd.Queries;
using Rebus.CastleWindsor;

namespace CoreBackend.Account.Register.Castle;

/// <summary>
/// Extension methods on <see cref="IWindsorContainer"/> for registering CoreBackend.Account services.
/// </summary>
public static class CoreBackendAccountServicesCastleWindsorRegistrationExtensions
{
    extension(IWindsorContainer hostedServiceWindsorContainer)
    {
        /// <summary>
        /// Registers CoreBackend.Account services into Castle Windsor.
        /// </summary>
        public void AddCoreBackendAccountServices()
        {
            hostedServiceWindsorContainer.Register(
                Classes
                    .FromAssemblyContaining<LoginTokenRepository>()
                    .BasedOn(typeof(IRepository<>))
                    .WithService.AllInterfaces()
                    .Configure(x => x.LifestyleTransient()),
                Classes
                    .FromAssemblyContaining<GetLoginTokenByIdQueryHandler>()
                    .BasedOn(typeof(IQueryHandler<,>))
                    .WithService.AllInterfaces()
                    .Configure(x => x.LifestyleTransient())
            );

            // Rebus message handlers from CoreBackend.Account
            hostedServiceWindsorContainer.AutoRegisterHandlersFromAssemblyOf<ConfirmLoginTokenCommandMessageHandler>();
        }
    }
}
