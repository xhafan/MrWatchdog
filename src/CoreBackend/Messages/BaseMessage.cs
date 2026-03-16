namespace CoreBackend.Messages;

public abstract record BaseMessage
{
    public long ActingUserId { get; set; }
    public string? RequestId { get; set; }
}