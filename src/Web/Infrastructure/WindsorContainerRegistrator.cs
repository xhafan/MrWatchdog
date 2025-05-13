using Castle.MicroKernel.Registration;
using Castle.MicroKernel.Registration.Lifestyle;
using Castle.Windsor;
using Castle.Windsor.Installer;
using CoreDdd.Nhibernate.Configurations;
using CoreDdd.Nhibernate.Register.Castle;
using CoreDdd.Register.Castle;
using CoreDdd.UnitOfWorks;

namespace MrWatchdog.Web.Infrastructure;

public static class WindsorContainerRegistrator
{
    public static void RegisterServices(
        IWindsorContainer windsorContainer,
        Func<LifestyleGroup<IUnitOfWork>, ComponentRegistration<IUnitOfWork>> setUnitOfWorkLifeStyleFunc,
        INhibernateConfigurator? nhibernateConfigurator = null
    )
    {
        CoreDddNhibernateInstaller.SetUnitOfWorkLifeStyle(setUnitOfWorkLifeStyleFunc);

        windsorContainer.Install(
            FromAssembly.Containing<CoreDddInstaller>(),
            FromAssembly.Containing<CoreDddNhibernateInstaller>()
        );

        if (nhibernateConfigurator != null)
        {
            windsorContainer.Register(
                Component.For<INhibernateConfigurator>()
                    .Instance(nhibernateConfigurator)
            );
        }
    }
}