﻿using MrWatchdog.Core.Features.Watchdogs.Domain;
using MrWatchdog.TestsShared;
using MrWatchdog.TestsShared.Builders;
using MrWatchdog.Web.Features.Watchdogs.Detail.WebPage;

namespace MrWatchdog.Web.Tests.Features.Watchdogs.Detail.WebPage;

[TestFixture]
public class when_viewing_watchdog_web_page_turbo_frame : BaseDatabaseTest
{
    private WebPageTurboFrameModel _model = null!;
    private Watchdog _watchdog = null!;
    private long _watchdogWebPageId;

    [SetUp]
    public async Task Context()
    {
        _BuildEntities();
        await UnitOfWork.FlushAsync();
        UnitOfWork.Clear();

        _model = new WebPageTurboFrameModel();

        _model.OnGet(_watchdog.Id, watchdogWebPageId: _watchdogWebPageId);
    }

    [Test]
    public void model_is_correct()
    {
        _model.WatchdogId.ShouldBe(_watchdog.Id);
        _model.WatchdogWebPageId.ShouldBe(_watchdogWebPageId);
    }  

    private void _BuildEntities()
    {
        _watchdog = new WatchdogBuilder(UnitOfWork)
            .WithWebPage(new WatchdogWebPageArgs
            {
                Url = "http://url.com/page",
                Selector = ".selector",
                Name = "url.com/page"
            })
            .Build();
        _watchdogWebPageId = _watchdog.WebPages.Single().Id;
    }    
}