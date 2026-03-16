using Rebus.Config;
using Rebus.Routing;
using System.Reflection;

namespace CoreBackend.Infrastructure.Rebus.MessageRouting;

public static class MessageRoutingConfigurator
{
    private static MessageRouter? _messageRouter;

    public static void ConfigureMessageRouting(
        StandardConfigurer<IRouter> routerConfigurer,
        string environmentName,
        IEnumerable<Assembly> assembliesWithTypesDerivedFromBaseMessage
    )
    {
        _messageRouter ??= new MessageRouter(environmentName, assembliesWithTypesDerivedFromBaseMessage);

        routerConfigurer.Register(_ => _messageRouter);
    }
}