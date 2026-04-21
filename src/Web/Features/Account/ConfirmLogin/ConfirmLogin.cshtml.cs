using CoreBackend.Account.Features.LoginLink;
using CoreBackend.Features.Jobs.Services;
using CoreBackend.Infrastructure.Rebus;
using CoreDdd.Queries;
using CoreWeb.Account.Features.LoginLink.ConfirmLogin;
using Microsoft.Extensions.Options;

namespace MrWatchdog.Web.Features.Account.ConfirmLogin;

public class ConfirmLoginModel(
    ICoreBus bus,
    IOptions<JwtOptions> iJwtOptions,
    IQueryExecutor queryExecutor,
    IJobCompletionAwaiter jobCompletionAwaiter
) 
    : BaseConfirmLoginModel(bus, iJwtOptions, queryExecutor, jobCompletionAwaiter);