using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace CoreBackend.Register.ServiceProvider;

/// <summary>
/// Holds a reference to the main ASP.NET Core <see cref="IServiceProvider"/>
/// so open generic implementations can access it via constructor injection.
/// </summary>
public class MainServiceProviderHolder(IServiceProvider serviceProvider)
{
    public IServiceProvider ServiceProvider => serviceProvider;
}

/// <summary>
/// Open generic <see cref="IOptions{T}"/> implementation that forwards resolution
/// to the main ASP.NET Core service provider. Registered as <c>typeof(IOptions&lt;&gt;)</c>
/// so any <c>IOptions&lt;T&gt;</c> automatically resolves from the main container.
/// </summary>
public class MainServiceProviderOptions<T>(MainServiceProviderHolder holder) : IOptions<T>
    where T : class
{
    public T Value => holder.ServiceProvider.GetRequiredService<IOptions<T>>().Value;
}
