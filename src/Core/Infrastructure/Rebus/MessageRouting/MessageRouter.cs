using System.Reflection;
using CoreUtils;
using MrWatchdog.Core.Messages;
using Rebus.Messages;
using Rebus.Routing;

namespace MrWatchdog.Core.Infrastructure.Rebus.MessageRouting;

public class MessageRouter : IRouter
{
    private readonly Dictionary<Type, string> _environmentQueueNamesByMessageType = [];

    public MessageRouter(string environmentName)
    {
        var allMessageTypes = _GetAllDerivedTypes<BaseMessage>();

        foreach (var messageType in allMessageTypes)
        {
            var rebusRoutingAttribute = messageType.GetCustomAttribute<RebusRoutingAttribute>(inherit: true);
            
            _mapMessageTypeToEnvironmentQueueName(
                messageType,
                rebusRoutingAttribute != null
                    ? $"{environmentName}{rebusRoutingAttribute.Queue}"
                    : $"{environmentName}{RebusQueues.Main}"
            );
        }
    }

    public Task<string> GetDestinationAddress(Message message)
    {
        var messageType = message.Body.GetType();
        var headers = message.Headers;

        var destinationAddress = Task.FromResult(headers.TryGetValue(CustomHeaders.QueueForRedirection, out var queueForRedirection) 
            ? queueForRedirection
            : _environmentQueueNamesByMessageType[messageType]);

        return destinationAddress;
    }

    private void _mapMessageTypeToEnvironmentQueueName(Type messageType, string environmentQueueName)
    {
        _environmentQueueNamesByMessageType[messageType] = environmentQueueName;
    }

    private static ICollection<Type> _GetAllDerivedTypes<TType>()
    {
        var allDerivedTypes = typeof(TType).Assembly.GetTypes()
            .Where(x => typeof(TType).IsAssignableFrom(x)
                        && !x.IsAbstract
                        && x.IsPublic)
            .ToList();

        Guard.Hope(allDerivedTypes.Any(), $"There are no derived types for type {typeof(TType).Name}");
        return allDerivedTypes;
    }

    public Task<string> GetOwnerAddress(string topic)
    {
        throw new NotImplementedException();
    }
}