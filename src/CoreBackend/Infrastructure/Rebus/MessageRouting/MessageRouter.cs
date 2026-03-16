using System.Reflection;
using CoreBackend.Messages;
using CoreUtils;
using Rebus.Messages;
using Rebus.Routing;

namespace CoreBackend.Infrastructure.Rebus.MessageRouting;

public class MessageRouter : IRouter
{
    private readonly Dictionary<Type, string> _environmentQueueNamesByMessageType = [];

    public MessageRouter(string environmentName, IEnumerable<Assembly> assembliesWithTypesDerivedFromBaseMessage)
    {
        var allMessageTypes = _GetAllDerivedTypes(assembliesWithTypesDerivedFromBaseMessage);

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

    private static ICollection<Type> _GetAllDerivedTypes(IEnumerable<Assembly> assembliesWithTypesDerivedFromBaseMessage)
    {
        var allDerivedTypes = assembliesWithTypesDerivedFromBaseMessage.SelectMany(x => x.GetTypes())
            .Where(x => typeof(BaseMessage).IsAssignableFrom(x)
                        && !x.IsAbstract
                        && x.IsPublic)
            .ToList();

        Guard.Hope(allDerivedTypes.Any(), $"There are no types derived from {nameof(BaseMessage)} in the assembly.");
        return allDerivedTypes;
    }

    public Task<string> GetOwnerAddress(string topic)
    {
        throw new NotImplementedException();
    }
}