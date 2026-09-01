# CoreWeb.Tests Log Test Migration Design

## Goal

Move the `when_logging_error` unit test out of `src/Web.Tests` and into a new `src/Libraries/CoreWeb/CoreWeb.Tests` .NET test project, alongside `CoreWeb` and `CoreWeb.Account`, whose production dependency is `CoreWeb`.

## Project boundary

`CoreWeb.Tests` will be a `net10.0` NUnit project following the existing MrWatchdog test-project conventions through `src/SharedAssemblyInfo.props`. It will reference:

- `src/Libraries/CoreWeb/CoreWeb/CoreWeb.csproj`, which owns `LogsController`.
- `src/Libraries/CoreBackend/CoreBackend.TestsShared/CoreBackend.TestsShared.csproj`, because the test must continue using `OptionsTestRetriever`.

The project will carry direct package references for NUnit, the NUnit adapter, the .NET test SDK, FakeItEasy, and Shouldly because its source uses those APIs directly. It will link the shared CoreBackend test global-usings file.

## Configuration

The migrated test will continue obtaining `LoggingOptions` and `EmailAddressesOptions` through `OptionsTestRetriever`. A project-level NUnit setup fixture will initialize `ConsoleAppSettings` against the new test assembly, which will use the same `UserSecretsId` as the existing MrWatchdog test projects. This preserves the configuration source used by the current passing test without pulling in `Core.TestsShared`, database setup, or the `Web` application project.

## Test migration

Move `src/Web.Tests/Features/Logs/when_logging_error.cs` to `src/Libraries/CoreWeb/CoreWeb.Tests/Features/Logs/when_logging_error.cs`. Preserve its setup, action, and assertions; change only its namespace from `MrWatchdog.Web.Tests.Features.Logs` to `CoreWeb.Tests.Features.Logs`.

## Repository integration

Add `CoreWeb.Tests.csproj` to `src/MrWatchdog.sln` under the existing `Tests` solution folder. Remove only the original test source from `Web.Tests`; do not otherwise change `Web.Tests.csproj` because its remaining tests still use all current references and test infrastructure.

List `CoreWeb.Tests` in the CoreWeb subtree README. Add the project's `.csproj` and full source directory to the matching restore-cache and test-source copy sections in `src/Dockerfile.tests`; the existing solution-wide Docker build and test commands will then include it automatically.

## Verification

- Establish the existing test baseline with its current fully qualified name.
- Run the migrated test by its new fully qualified name from `CoreWeb.Tests.csproj`.
- Build the new project and confirm it is listed in the solution under `Tests`.
- Run Dockerfile static validation and build the Docker test image's build stage when existing dependency-audit state permits.
- Run whitespace and final-diff checks to ensure the move, project wiring, setup fixture, and design records are the only changes.
