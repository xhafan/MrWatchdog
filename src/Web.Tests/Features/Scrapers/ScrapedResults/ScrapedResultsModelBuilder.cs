using System.Security.Claims;
using CoreDdd.Nhibernate.UnitOfWorks;
using CoreDdd.Queries;
using FakeItEasy;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using MrWatchdog.Core.Features.Account.Domain;
using MrWatchdog.Core.Features.Scrapers.Domain;
using MrWatchdog.Core.Features.Scrapers.Queries;
using MrWatchdog.Core.Infrastructure.Rebus;
using MrWatchdog.Core.Infrastructure.Repositories;
using MrWatchdog.TestsShared;
using MrWatchdog.Web.Features.Scrapers.ScrapedResults;

namespace MrWatchdog.Web.Tests.Features.Scrapers.ScrapedResults;

public class ScrapedResultsModelBuilder(NhibernateUnitOfWork unitOfWork)
{
    private ICoreBus? _bus;
    private IAuthorizationService? _authorizationService;
    private User? _actingUser;
    private string? _searchTerm;

    public ScrapedResultsModelBuilder WithBus(ICoreBus bus)
    {
        _bus = bus;
        return this;
    }   

    public ScrapedResultsModelBuilder WithAuthorizationService(IAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
        return this;
    }

    public ScrapedResultsModelBuilder WithActingUser(User actingUser)
    {
        _actingUser = actingUser;
        return this;
    }

    public ScrapedResultsModelBuilder WithSearchTerm(string? searchTerm)
    {
        _searchTerm = searchTerm;
        return this;
    }

    public ScrapedResultsModel Build()
    {
        _bus ??= A.Fake<ICoreBus>();

        var queryHandlerFactory = new FakeQueryHandlerFactory();
        
        queryHandlerFactory.RegisterQueryHandler(new GetScraperScrapedResultsArgsQueryHandler(
            unitOfWork,
            new NhibernateRepository<Scraper>(unitOfWork)
        ));

        if (_authorizationService == null)
        {
            _authorizationService = A.Fake<IAuthorizationService>();
            A.CallTo(() => _authorizationService.AuthorizeAsync(A<ClaimsPrincipal>._, A<long>._, A<IAuthorizationRequirement[]>._))
                .Returns(AuthorizationResult.Success());
        }

        var model = new ScrapedResultsModel(
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