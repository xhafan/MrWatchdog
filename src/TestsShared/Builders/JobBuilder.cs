using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Jobs.Domain;
using MrWatchdog.Core.Features.Watchdogs.Commands;
using MrWatchdog.TestsShared.Extensions;

namespace MrWatchdog.TestsShared.Builders;

public class JobBuilder(NhibernateUnitOfWork? unitOfWork = null)
{
    public const string Type = nameof(CreateWatchdogCommand);
    public static readonly object InputData = new CreateWatchdogCommand(23, "watchdog name");
    public const JobKind Kind = JobKind.Command;
    public const string RequestId = "0HNFBP8T98MQS:00000045";

    private Guid _guid;
    private string _type = Type;
    private object _inputData = InputData;
    private JobKind _kind = Kind;
    private string? _requestId = RequestId;

    public JobBuilder WithGuid(Guid guid)
    {
        _guid = guid;
        return this;
    }
    
    public JobBuilder WithType(string type)
    {
        _type = type;
        return this;
    }
    
    public JobBuilder WithInputData(object inputData)
    {
        _inputData = inputData;
        return this;
    }
    
    public JobBuilder WithKind(JobKind kind)
    {
        _kind = kind;
        return this;
    }    
    
    public JobBuilder WithRequestId(string? requestId)
    {
        _requestId = requestId;
        return this;
    }        
    
    public Job Build()
    {
        if (_guid == Guid.Empty)
        {
            _guid = Guid.NewGuid();
        }
        
        var job = new Job(
            _guid,
            _type,
            _inputData,
            _kind,
            _requestId
        );

        unitOfWork?.Save(job);
        
        return job;
    }
}