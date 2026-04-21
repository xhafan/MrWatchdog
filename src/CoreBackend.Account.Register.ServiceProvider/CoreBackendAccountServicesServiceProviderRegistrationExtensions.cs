using CoreBackend.Account.Features.LoginLink.Commands;
using CoreBackend.Account.Features.LoginLink.Queries;
using CoreBackend.Account.Infrastructure.Repositories;
using CoreDdd.Domain.Repositories;
using CoreDdd.Queries;
using Microsoft.Extensions.DependencyInjection;
using Rebus.Handlers;

namespace CoreBackend.Account.Register.ServiceProvider;

/// <summary>
/// Extension methods on <see cref="IServiceCollection"/> for registering CoreBackend.Account services.
/// </summary>
public static class CoreBackendAccountServicesServiceProviderRegistrationExtensions
{
    extension(IServiceCollection hostedServiceServices)
    {
        /// <summary>
        /// Registers all CoreBackend.Account services into ServiceProvider.
        /// </summary>
        public void AddCoreBackendAccountServices()
        {
            hostedServiceServices.AddCoreBackendAccountRepositoriesAndQueryHandlers();

            // Rebus message handlers from CoreBackend.Account
            hostedServiceServices.Scan(scan => scan
                .FromAssemblyOf<ConfirmLoginTokenCommandMessageHandler>()
                .AddClasses(classes => classes.AssignableTo(typeof(IHandleMessages<>)))
                .AsImplementedInterfaces()
                .WithTransientLifetime()
            );
        }

        public void AddCoreBackendAccountRepositoriesAndQueryHandlers()
        {
            hostedServiceServices.Scan(scan => scan
                .FromAssemblyOf<LoginTokenRepository>()
                .AddClasses(classes => classes.AssignableTo(typeof(IRepository<>)))
                .AsImplementedInterfaces()
                .WithTransientLifetime()
            );
            hostedServiceServices.Scan(scan => scan
                .FromAssemblyOf<GetLoginTokenByIdQueryHandler>()
                .AddClasses(classes => classes.AssignableTo(typeof(IQueryHandler<,>)))
                .AsImplementedInterfaces()
                .WithTransientLifetime()
            );
        }
    }
}