using Microsoft.AspNetCore.Mvc.ApplicationModels;
using MrWatchdog.Web.Infrastructure;

namespace MrWatchdog.Web.Tests.Infrastructure.PageRouteModelConventions;

[TestFixture]
public class when_applying_page_route_model_convention
{
    [TestCase(
        "/Features/Scrapers/Create/Create.cshtml",
        "/Scrapers/Create/Create",
        "Scrapers/Create/Create",
        "Scrapers/Create",
        TestName = "1 Razor page without parameters")]
    [TestCase(
        "/Features/Scrapers/Detail/Detail.cshtml",
        "/Scrapers/Detail/Detail",
        "Scrapers/Detail/Detail/{id}",
        "Scrapers/Detail/{id}",
        TestName = "2 Razor page with parameters")]
    [TestCase(
        "/Features/Scrapers/Detail/Overview/Overview.cshtml",
        "/Scrapers/Detail/Overview/Overview",
        "Scrapers/Detail/Overview/Overview/{id}",
        "Scrapers/Detail/Overview/{id}",
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
