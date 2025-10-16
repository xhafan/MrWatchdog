﻿using System.Net;
using System.Text;
using CoreUtils;
using Microsoft.Extensions.Configuration;
using MrWatchdog.Core.Infrastructure;
using MrWatchdog.TestsShared;

namespace MrWatchdog.Web.E2E.Tests.Features.Logs;

[TestFixture]
public class when_logging_error : BaseDatabaseTest
{
    [Test]
    public async Task error_message_can_be_logged_unauthenticated_with_a_secret_in_header()
    {
        var config = new ConfigurationBuilder()
            .AddUserSecrets<when_logging_error>()
            .Build();

        var logErrorApiSecret = config["Logging:LogErrorApiSecret"];
        Guard.Hope(logErrorApiSecret != null, nameof(logErrorApiSecret) + " is null");

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/Logs/LogError");
        request.Headers.Add(LogConstants.LogErrorApiSecretHeaderName, logErrorApiSecret);
        request.Content = new StringContent(
            JsonHelper.Serialize("JS error message"),
            Encoding.UTF8,
            "application/json"
        );
        var response = await RunOncePerTestRun.SharedWebApplicationClient.Value.SendAsync(request);
        
        response.StatusCode.ShouldBe(HttpStatusCode.OK);
    }

    [Test]
    public async Task error_message_cannot_be_logged_without_a_secret_in_header()
    {
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/Logs/LogError");
        request.Content = new StringContent(
            JsonHelper.Serialize("JS error message"),
            Encoding.UTF8,
            "application/json"
        );
        var response = await RunOncePerTestRun.SharedWebApplicationClient.Value.SendAsync(request);
        
        response.StatusCode.ShouldBe(HttpStatusCode.Forbidden);
    }
}