using CoreDdd.Nhibernate.UnitOfWorks;
using CoreDdd.Queries;
using FakeItEasy;
using Microsoft.AspNetCore.Authorization;
using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Queries;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.Web.Features.Watchdogs.ScrapingResults;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using MrWatchdog.Core.Features.Account.Domain;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.ScrapingResults;

public class ScrapingResultsModelBuilder(NhibernateUnitOfWork unitOfWork)
{
    private ICoreBus? _bus;
    private IAuthorizationService? _authorizationService;
    private User? _actingUser;
    private string? _searchTerm;

    public ScrapingResultsModelBuilder WithBus(ICoreBus bus)
    {
        _bus = bus;
        return this;
    }   

    public ScrapingResultsModelBuilder WithAuthorizationService(IAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
        return this;
    }

    public ScrapingResultsModelBuilder WithActingUser(User actingUser)
    {
        _actingUser = actingUser;
        return this;
    }

    public ScrapingResultsModelBuilder WithSearchTerm(string? searchTerm)
    {
        _searchTerm = searchTerm;
        return this;
    }

    public ScrapingResultsModel Build()
    {
        _bus ??= A.Fake<ICoreBus>();

        var queryHandlerFactory = new FakeQueryHandlerFactory();
        
        queryHandlerFactory.RegisterQueryHandler(new GetWatchdogScrapingResultsArgsQueryHandler(
            unitOfWork,
            new NhibernateRepository<Watchdog>(unitOfWork)
        ));

        if (_authorizationService == null)
        {
            _authorizationService = A.Fake<IAuthorizationService>();
            A.CallTo(() => _authorizationService.AuthorizeAsync(A<ClaimsPrincipal>._, A<long>._, A<IAuthorizationRequirement[]>._))
                .Returns(AuthorizationResult.Success());
        }

        var model = new ScrapingResultsModel(
            new QueryExecutor(queryHandlerFactory),
            _bus,
            _authorizationService
        )
        {
            SearchTerm = _searchTerm
        };

        ModelValidator.ValidateModel(model);

        _setupModelUser();

        return model;

        void _setupModelUser()
        {
            IEnumerable<Claim> claims = _actingUser != null 
                ? [new Claim(ClaimTypes.NameIdentifier, $"{_actingUser.Id}")] 
                : [];

            model.PageContext = new PageContext(new ActionContext(new DefaultHttpContext
                {
                    User = new ClaimsPrincipal(new ClaimsIdentity(
                        claims,
                        authenticationType: _actingUser != null
                            ? CookieAuthenticationDefaults.AuthenticationScheme
                            : null)
                    )
                },
                new RouteData(),
                new ActionDescriptor(),
                model.ModelState)
            );
        }
    }
}