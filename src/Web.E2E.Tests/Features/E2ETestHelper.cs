using System.Net;
using System.Text.RegularExpressions;
using CoreDdd.Nhibernate.UnitOfWorks;
using MrWatchdog.Core.Features.Account;
using MrWatchdog.Core.Features.Account.Commands;
using MrWatchdog.Core.Features.Jobs.Domain;
using MrWatchdog.Core.Infrastructure.EmailSenders;
using MrWatchdog.TestsShared;
using NHibernate;
using NHibernate.Criterion;

namespace MrWatchdog.Web.E2E.Tests.Features;

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

    public static async Task LogUserIn(
        HttpClient webApplicationClient, 
        Guid loginTokenGuid,
        Dictionary<string, string>? httpHeaders = null
    )
    {
        var request = new HttpRequestMessage(
            HttpMethod.Post,
            AccountUrlConstants.ApiCompleteLoginUrlTemplate.WithLoginTokenGuid(loginTokenGuid)
        );

        if (httpHeaders != null)
        {
            foreach (var httpHeader in httpHeaders)
            {
                request.Headers.Add(httpHeader.Key, httpHeader.Value);
            }
        }
        
        var response = await webApplicationClient.SendAsync(request);
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
    
    public static async Task DeleteScraperCommandJob<TCommand>(long scraperId, NhibernateUnitOfWork unitOfWork)
    {
        var commandJob = await unitOfWork.Session!.QueryOver<Job>()
            .Where(x => x.Type == typeof(TCommand).Name)
            .And(Expression.Sql(
                """
                ({alias}."InputData" ->> 'scraperId') = ?
                """,
                scraperId.ToString(),
                NHibernateUtil.String)
            )
            .SingleOrDefaultAsync();
        await unitOfWork.DeleteJobCascade(commandJob, waitForJobCompletion: true);
    }

    public static async Task DeleteCommandJob<TCommand>(Guid commandGuid, NhibernateUnitOfWork unitOfWork)
    {
        var commandJob = await unitOfWork.Session!.QueryOver<Job>()
            .Where(x => x.Type == typeof(TCommand).Name
                        && x.Guid == commandGuid)
            .SingleOrDefaultAsync();
        await unitOfWork.DeleteJobCascade(commandJob, waitForJobCompletion: true);
    }

    public static async Task DeleteSendEmailCommandJob(string substringOfHtmlEmail, NhibernateUnitOfWork unitOfWork)
    {
        var commandJob = await unitOfWork.Session!.QueryOver<Job>()
            .Where(x => x.Type == nameof(SendEmailCommand))
            .And(Expression.Sql(
                """
                ({alias}."InputData" ->> 'htmlMessage') like ?
                """,
                $"%{substringOfHtmlEmail}%",
                NHibernateUtil.String)
            )
            .SingleOrDefaultAsync();
        await unitOfWork.DeleteJobCascade(commandJob, waitForJobCompletion: true);
    }
}