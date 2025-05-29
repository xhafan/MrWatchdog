using Microsoft.AspNetCore.Mvc.ApplicationModels;
using MrWatchdog.Web.Infrastructure;

namespace MrWatchdog.Web.Tests.Infrastructure.PageRouteModelConventions;

[TestFixture]
public class when_applying_page_route_model_convention
{
    [TestCase(
        "/Features/Watchdogs/Create/Create.cshtml",
        "/Watchdogs/Create/Create",
        "Watchdogs/Create/Create",
        "Watchdogs/Create",
        TestName = "1 Razor page without parameters")]
    [TestCase(
        "/Features/Watchdogs/Detail/Detail.cshtml",
        "/Watchdogs/Detail/Detail",
        "Watchdogs/Detail/Detail/{id}",
        "Watchdogs/Detail/{id}",
        TestName = "2 Razor page with parameters")]
    [TestCase(
        "/Features/Watchdogs/Detail/Overview/Overview.cshtml",
        "/Watchdogs/Detail/Overview/Overview",
        "Watchdogs/Detail/Overview/Overview/{id}",
        "Watchdogs/Detail/Overview/{id}",
        TestName = "3 Nested razor page")]
    public void selector_template_is_correct(
        string relativePath, 
        string viewEnginePath, 
        string selectorTemplate,
        string expectedSelectorTemplate
        )
    {
        var pageRouteModelConvention = new PageRouteModelConvention();

        var pageRouteModel = new PageRouteModel(relativePath, viewEnginePath);
        pageRouteModel.Selectors.Add(new SelectorModel { AttributeRouteModel = new AttributeRouteModel { Template = selectorTemplate } });

        pageRouteModelConvention.Apply(pageRouteModel);

        pageRouteModel.Selectors.Single().AttributeRouteModel!.Template.ShouldBe(expectedSelectorTemplate);
    }
}
