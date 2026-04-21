using CoreBackend.Features.Account;
using CoreDdd.Queries;
using CoreWeb.Features.Account.LoginLinkSent;
using Microsoft.Extensions.Options;

namespace MrWatchdog.Web.Features.Account.LoginLinkSent;

public class LoginLinkSentModel(
    IQueryExecutor queryExecutor,
    IOptions<JwtOptions> iJwtOptions
) 
    : BaseLoginLinkSentModel(queryExecutor, iJwtOptions);