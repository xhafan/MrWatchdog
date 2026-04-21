using CoreBackend.Account.Features.LoginLink;
using CoreDdd.Queries;
using CoreWeb.Account.Features.LoginLink.LoginLinkSent;
using Microsoft.Extensions.Options;

namespace MrWatchdog.Web.Features.Account.LoginLinkSent;

public class LoginLinkSentModel(
    IQueryExecutor queryExecutor,
    IOptions<JwtOptions> iJwtOptions
) 
    : BaseLoginLinkSentModel(queryExecutor, iJwtOptions);