# CoreWeb.Tests Log Test Migration Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. MrWatchdog repository rules require inline execution in the existing `main` checkout and prohibit subagents, worktrees, commits, and branches.

**Goal:** Move the CoreWeb log-controller test into a dedicated `src/Libraries/CoreWeb/CoreWeb.Tests` project while retaining `OptionsTestRetriever`.

**Architecture:** Create a focused NUnit project that references `CoreWeb` and `CoreBackend.TestsShared`, initializes the shared configuration reader without database setup, and owns the migrated log test. Register the project in the existing solution `Tests` folder.

**Tech Stack:** .NET 10, NUnit 4.5, NUnit3TestAdapter 6.1, Microsoft.NET.Test.Sdk 18.0, FakeItEasy 9.0, Shouldly 4.3, ASP.NET Core MVC

**Spec:** `docs/superpowers/specs/2026-09-01-coreweb-tests-log-migration-design.md`

## Global Constraints

- Work directly in the existing MrWatchdog `main` checkout.
- Do not create commits, branches, worktrees, or subagents.
- Keep using `CoreBackend.TestsShared.OptionsTestRetriever` in the migrated test.
- Reference `CoreWeb` and `CoreBackend.TestsShared`; do not reference `Web` or `Core.TestsShared`.
- Preserve the existing test behavior and assertions.

---

### Task 1: Create and verify CoreWeb.Tests

**Files:**

- Create: `src/Libraries/CoreWeb/CoreWeb.Tests/CoreWeb.Tests.csproj`
- Create: `src/Libraries/CoreWeb/CoreWeb.Tests/RunOncePerTestRun.cs`
- Create: `src/Libraries/CoreWeb/CoreWeb.Tests/Features/Logs/when_logging_error.cs`
- Delete: `src/Web.Tests/Features/Logs/when_logging_error.cs`
- Modify: `src/Libraries/CoreWeb/README.md`
- Modify: `src/Dockerfile.tests`
- Modify: `src/MrWatchdog.sln`

**Interfaces:**

- Consumes: `CoreWeb.Features.Logs.LogsController`, `CoreBackend.TestsShared.OptionsTestRetriever`, and the existing test configuration identified by MrWatchdog's shared `UserSecretsId`.
- Produces: a solution test project named `CoreWeb.Tests` and the NUnit fixture `CoreWeb.Tests.Features.Logs.when_logging_error`.

- [x] **Step 1: Confirm the existing test baseline**

Run from `src`:

```powershell
dotnet test Web.Tests/Web.Tests.csproj --filter "FullyQualifiedName~MrWatchdog.Web.Tests.Features.Logs.when_logging_error" --no-restore
```

Expected: exit code 0 with exactly one passing test.

- [x] **Step 2: Create the test project**

Create `src/Libraries/CoreWeb/CoreWeb.Tests/CoreWeb.Tests.csproj`:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <RootNamespace>CoreWeb.Tests</RootNamespace>
    <AssemblyName>CoreWeb.Tests</AssemblyName>
    <UserSecretsId>9415da77-0c91-4852-a86c-51b447fef23b</UserSecretsId>
  </PropertyGroup>

  <Import Project="..\..\..\SharedAssemblyInfo.props" />

  <ItemGroup>
    <Compile Include="..\..\CoreBackend\CoreBackend.TestsShared\GlobalUsings.cs" Link="GlobalUsings.cs" />
  </ItemGroup>

  <ItemGroup>
    <FrameworkReference Include="Microsoft.AspNetCore.App" />
  </ItemGroup>

  <ItemGroup>
    <PackageReference Include="FakeItEasy" Version="9.0.1" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="18.0.1" />
    <PackageReference Include="NUnit" Version="4.5.0" />
    <PackageReference Include="NUnit3TestAdapter" Version="6.1.0" />
    <PackageReference Include="Shouldly" Version="4.3.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\CoreBackend\CoreBackend.TestsShared\CoreBackend.TestsShared.csproj" />
    <ProjectReference Include="..\CoreWeb\CoreWeb.csproj" />
  </ItemGroup>

</Project>
```

- [x] **Step 3: Initialize configuration for OptionsTestRetriever**

Create `src/Libraries/CoreWeb/CoreWeb.Tests/RunOncePerTestRun.cs`:

```csharp
using CoreBackend.Infrastructure.Configurations;

namespace CoreWeb.Tests;

[SetUpFixture]
public class RunOncePerTestRun
{
    [OneTimeSetUp]
    public void SetUp()
    {
        ConsoleAppSettings.Initialize(typeof(RunOncePerTestRun).Assembly);
    }
}
```

- [x] **Step 4: Move the existing test**

Move `src/Web.Tests/Features/Logs/when_logging_error.cs` to `src/Libraries/CoreWeb/CoreWeb.Tests/Features/Logs/when_logging_error.cs`, preserving its source except for this namespace change:

```csharp
namespace CoreWeb.Tests.Features.Logs;
```

- [x] **Step 5: Register CoreWeb.Tests in the solution**

Run from `src`:

```powershell
dotnet sln MrWatchdog.sln add Libraries/CoreWeb/CoreWeb.Tests/CoreWeb.Tests.csproj --solution-folder Tests
```

Expected: exit code 0, a `CoreWeb.Tests` project entry with all solution build configurations, and a nested-project entry pointing at the existing `Tests` folder.

- [x] **Step 6: Verify the migrated test and project**

Run from `src`:

```powershell
dotnet test Libraries/CoreWeb/CoreWeb.Tests/CoreWeb.Tests.csproj --filter "FullyQualifiedName~CoreWeb.Tests.Features.Logs.when_logging_error"
dotnet build Libraries/CoreWeb/CoreWeb.Tests/CoreWeb.Tests.csproj --no-restore
dotnet sln MrWatchdog.sln list
```

Expected: the focused test reports one pass, the project builds with zero errors, and the solution list contains `Libraries\CoreWeb\CoreWeb.Tests\CoreWeb.Tests.csproj`.

- [x] **Step 7: Add the project to the CoreWeb README and Docker test build**

Add `CoreWeb.Tests` to the project list in `src/Libraries/CoreWeb/README.md`. Add these lines to the corresponding project-manifest and full-source copy sections in `src/Dockerfile.tests`:

```dockerfile
COPY Libraries/CoreWeb/CoreWeb.Tests/*.csproj Libraries/CoreWeb/CoreWeb.Tests/
COPY Libraries/CoreWeb/CoreWeb.Tests/. ./Libraries/CoreWeb/CoreWeb.Tests/
```

Run from `src`:

```powershell
docker build --check --file Dockerfile.tests .
```

Expected: Docker reports `Check complete, no warnings found.`

- [x] **Step 8: Inspect the final repository state**

Run from the repository root:

```powershell
git status --short
git diff --check
git diff -- src/Dockerfile.tests src/MrWatchdog.sln src/Web.Tests/Features/Logs/when_logging_error.cs
git status --short -- src/Libraries/CoreWeb/CoreWeb.Tests src/Libraries/CoreWeb/README.md docs/superpowers
```

Expected: the original test is deleted, its replacement and new project infrastructure are added, the solution is updated, the approved design/plan records are present, and there are no whitespace errors or unrelated changes.
