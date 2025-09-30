using System.Security.Claims;
using FakeItEasy;
using Microsoft.AspNetCore.Authorization;
using MrWatchdog.Web.Features.Watchdogs.Detail.WebPage;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.Detail.WebPage;

public class WebPageTurboFrameModelBuilder
{
    private IAuthorizationService? _authorizationService;
    private long _watchdogId;
    private long _watchdogWebPageId;

    public WebPageTurboFrameModelBuilder WithAuthorizationService(IAuthorizationService authorizationService)
    {
        _authorizationService = authorizationService;
        return this;
    }
    
    public WebPageTurboFrameModelBuilder WithWatchdogId(long watchdogId)
    {
        _watchdogId = watchdogId;
        return this;
    }       
    
    public WebPageTurboFrameModelBuilder WithWatchdogWebPageId(long watchdogWebPageId)
    {
        _watchdogWebPageId = watchdogWebPageId;
        return this;
    } 

    public WebPageTurboFrameModel Build()
    {
        if (_authorizationService == null)
        {
            _authorizationService = A.Fake<IAuthorizationService>();
            A.CallTo(() => _authorizationService.AuthorizeAsync(A<ClaimsPrincipal>._, A<long>._, A<IAuthorizationRequirement[]>._))
                .Returns(AuthorizationResult.Success());
        }

        var model = new WebPageTurboFrameModel(
            _authorizationService
        )
        {
            WatchdogId = _watchdogId,
            WatchdogWebPageId = _watchdogWebPageId
        };
        ModelValidator.ValidateModel(model);
        return model;
    }
}