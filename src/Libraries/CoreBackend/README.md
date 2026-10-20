# CoreBackend

Reusable backend projects extracted from [MrWatchdog](https://github.com/xhafan/MrWatchdog).

## Projects

- `CoreBackend` — core backend domain, messaging, infrastructure
- `CoreBackend.Account` — account/login-link domain on top of `CoreBackend`
- `CoreBackend.Postgres` — Postgres provider for `CoreBackend`
- `CoreBackend.Register.Castle` / `CoreBackend.Register.ServiceProvider` — DI registration for Castle Windsor and `Microsoft.Extensions.DependencyInjection`
- `CoreBackend.Account.Register.Castle` / `CoreBackend.Account.Register.ServiceProvider` — DI registration for the account projects
- `CoreBackend.TestsShared` — NUnit/FakeItEasy/Shouldly test helpers

## Consumption

Distributed via `git subtree`. Consumers vendor this repo into `src/Libraries/CoreBackend/` and reference individual `.csproj` files.

```bash
git subtree add  --prefix=src/Libraries/CoreBackend https://github.com/xhafan/CoreBackend main --squash
git subtree pull --prefix=src/Libraries/CoreBackend https://github.com/xhafan/CoreBackend main --squash
git subtree push --prefix=src/Libraries/CoreBackend https://github.com/xhafan/CoreBackend main
```

Consumers typically wrap these in a helper script — see [MrWatchdog's `src/subtree.sh`](https://github.com/xhafan/MrWatchdog/blob/main/src/subtree.sh) for a reference implementation.

This repo does not build standalone — cross-project references resolve only inside a consumer workspace. Develop inside a consumer, push changes back with `git subtree push`.

## Error reporting

Register a singleton `IErrorReporter` (for example `SlackErrorReporter`) and call
`services.AddErrorReporting(configuration)`. Add the registered `ErrorReportingSink`
to the host's Serilog configuration and enable `.Enrich.FromLogContext()` so suppression
and correlation properties reach the sink. Resolve the sink from the host service provider
in the Serilog configuration callback; do not build a second service provider.

`ErrorReporting` options default to a capacity of 256 reports, a 10-second delivery
timeout, and a 5-second shutdown drain. Set `Enabled` to false to disable queued
delivery. Logging never waits for queue space or remote delivery. Overflow, timeouts,
and delivery failures produce suppressed diagnostics without report contents or
exception messages that could expose webhook credentials. Delivery has no automatic
retries, and the queue is in-process; abrupt termination can lose pending reports.

The host selects its transport and destination. Slack uses `SlackErrorReporting` options
(`WebHookUrl`, `Channel`, `ApplicationName`); an empty channel disables HTTP delivery.
Use a dedicated `HttpClient` with a finite timeout, no retry policy, and disabled HTTP
request logging for webhook URLs. `ApplicationName` defaults to `Application`.

Reports retain an `Origin` (`Server`, `Browser`, or `FailedJob`) and optional
`FailedMessage` metadata. CoreWeb marks browser logs with `ErrorReportingEventIds.BrowserError`;
the sink classifies other log events as server errors. Rebus preserves the failed job's
GUID, message type, request ID, exception details, and timestamp through the queue.

For email delivery, register `IErrorReporter` as `EmailErrorReporter` in the main provider.
It uses `EmailAddressesOptions.FrontendErrors` for browser reports and `BackendErrors`
for server errors and failed jobs. Failed-job emails retain the `Job <type> failed`
subject and job link built from `RuntimeOptions.Url` and `JobUrlConstants`.
The main provider must supply an `ICoreBus` backed by the sending bus and a
`NewTransactionJobCreator`, so delivery works outside an HTTP request or failed transaction.
Register the sending Rebus service first, then `AddErrorReporting`, then the receiving
services: shutdown stops receivers, drains reports, and finally stops the sending bus.

Error-notification emails carry `SendEmailCommand.IsErrorNotification`. Their own
terminal failures remain in ordinary logs and do not generate another notification.
Unmarked email commands still receive normal failed-job reporting.

Application code, including CoreWeb's browser-error endpoint, only logs errors through
`ILogger`; the sink queues their notifications. Application handlers do not need an
explicit `IErrorReporter` call or a suppression scope. Reporting infrastructure and the
Rebus pipeline use `ErrorReportingLogPolicy.Suppress()` to prevent duplicate or recursive
forwarding while preserving ordinary log output.

For Rebus, install `SuppressErrorReportingDuringMessageHandlingStep` before
`DeserializeIncomingMessageStep` and decorate `IErrorHandler` with
`ReportFailedMessageErrorHandler`. Both hosted-service registration extensions register
`QueuedFailedMessageReporter` as their singleton `IFailedMessageReporter`. The existing CoreWeb Rebus
host installs the attempt step. Failed attempts remain in ordinary logs; a successful
retry produces no notification. A final report is queued after acknowledgement of the
poison message, including the message ID/type, request ID, and exception details.
Reporting failures never cause another delivery attempt.

When Rebus uses a separate service provider or Castle Windsor container, call
`AddCoreBackendServicesFromMainServiceProvider(mainServiceProvider)` as usual. Both
bridges require and import the main provider's `ErrorReportQueue`, so terminal
failures use its existing queue and delivery worker. Call `AddErrorReporting` in the
main provider only; it registers the queue, sink, and delivery worker without registering
`IFailedMessageReporter`. The failed-message reporter belongs to each Rebus container.
These registrations are unconditional and do not select an email fallback.
