using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Account.Commands;
using MrWatchdog.Core.Features.Jobs.Domain;
using MrWatchdog.Core.Features.Watchdogs;
using MrWatchdog.TestsShared;
using NHibernate;
using NHibernate.Criterion;
using System.Net;
using System.Text.RegularExpressions;

namespace MrWatchdog.Web.Tests.Features;

public static partial class E2ETestHelper
{
    [GeneratedRegex("__RequestVerificationToken\"[^>]+value=\"(?<token>[a-zA-Z0-9-_]+)\"")]
    private static partial Regex RequestVerificationTokenRegex();

    public static string ExtractRequestVerificationToken(string html)
    {
        return RequestVerificationTokenRegex().Match(html).Groups["token"].Value;
    }

    public static FormUrlEncodedContent GetFormUrlEncodedContentWithRequestVerificationToken(string requestVerificationToken)
    {
        return new FormUrlEncodedContent(new Dictionary<string, string>
        {
            {"__RequestVerificationToken", requestVerificationToken},
        });
    }

    public static async Task LogUserIn(HttpClient webApplicationClient, Guid loginTokenGuid)
    {
        var response = await webApplicationClient.PostAsync(
            AccountUrlConstants.ApiCompleteLoginUrlTemplate.WithLoginTokenGuid(loginTokenGuid), content: null);
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    public static async Task DeleteMarkLoginTokenAsUsedCommandJob(Guid loginTokenGuid, NhibernateUnitOfWork unitOfWork)
    {
        var markLoginTokenAsUsedCommandJob = await unitOfWork.Session!.QueryOver<Job>()
            .Where(x => x.Type == nameof(MarkLoginTokenAsUsedCommand))
            .And(Expression.Sql(
                """
                ({alias}."InputData" ->> 'loginTokenGuid') = ?
                """,
                loginTokenGuid.ToString(),
                NHibernateUtil.String)
            )
            .SingleOrDefaultAsync();
        await unitOfWork.DeleteJobCascade(markLoginTokenAsUsedCommandJob, waitForJobCompletion: true);
    }
    
    public static async Task DeleteWatchdogCommandJob<TCommand>(long watchdogId, NhibernateUnitOfWork unitOfWork)
    {
        var commandJob = await unitOfWork.Session!.QueryOver<Job>()
            .Where(x => x.Type == typeof(TCommand).Name)
            .And(Expression.Sql(
                """
                ({alias}."InputData" ->> 'watchdogId') = ?
                """,
                watchdogId.ToString(),
                NHibernateUtil.String)
            )
            .SingleOrDefaultAsync();
        await unitOfWork.DeleteJobCascade(commandJob, waitForJobCompletion: true);
    }    
}