using CoreDdd.Queries;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using MrWatchdog.Core.Features.Account.Queries;
using MrWatchdog.Core.Infrastructure.Validations;
using System.ComponentModel.DataAnnotations;

namespace MrWatchdog.Web.Features.Account;

[ApiController]
[Route("api/[controller]/[action]")]
public class LoginController(IQueryExecutor queryExecutor) : ControllerBase
{
    [HttpGet]
    public async Task<bool> GetLoginTokenConfirmation([Required, NotDefault]Guid loginTokenGuid)
    {
        return await queryExecutor.ExecuteSingleAsync<GetLoginTokenConfirmationQuery, bool>(new GetLoginTokenConfirmationQuery(loginTokenGuid));
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        return Redirect("/");
    }
}