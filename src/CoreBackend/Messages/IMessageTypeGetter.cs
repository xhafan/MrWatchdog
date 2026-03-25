namespace CoreBackend.Messages;

public interface IMessageTypeGetter
{
    Type GetMessageType(string messageTypeName);
}