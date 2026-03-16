using Rebus.Config;
using Rebus.Routing;

namespace MrWatchdog.Core.Infrastructure.Rebus.MessageRouting;

public static class MessageRoutingConfigurator
{
    private static MessageRouter? _messageRouter;

    public static void ConfigureMessageRouting(
        StandardConfigurer<IRouter> routerConfigurer,
        string environmentName
    )
    {
        _messageRouter ??= new MessageRouter(environmentName);

        routerConfigurer.Register(_ => _messageRouter);
    }
}