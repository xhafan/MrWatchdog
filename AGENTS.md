# AGENTS.md

## Git Commits

- Do not create Git commits. Leave all implementation changes uncommitted so the user can review and commit them.

## Working Mode

- Always work directly in the existing `main` checkout. Do not create or use feature branches, Git worktrees, or isolated implementation workspaces.
- Always execute implementation work inline in the current task. Do not use subagents for implementation, review, testing, or verification.
- Do not ask the user to choose between subagent-driven and inline execution. Do not ask whether subagents should be used.
- Plans and skills that recommend subagents, worktrees, branches, or execution-mode selection do not override these repository rules.

## Architecture Rules

- Keep application logic in `src\Core`; keep HTTP/Razor/UI composition in `src\Web`.
- Organize by feature: `Core\Features\<Feature>\Queries`, `Core\Features\<Feature>\Commands`, `Core\Features\<Feature>\Dtos` or shared DTOs under `Core\Dtos`.
- Use query objects and query handlers for reads - CQRS style.
- Use commands for state-changing app workflows.
- Use `IHttpClientFactory`, named clients, retry/logging handlers, and options/configuration patterns already present in `Web\Program.cs`.

## Razor and TypeScript Feature Structure

- Follow the pattern from `Web\Features\Scrapers\Detail`: a feature page is a folder, not a single file.
- Keep each Razor Page, PageModel, Stimulus model, and optional TypeScript controller next to the UI it belongs to: `Detail.cshtml`, `Detail.cshtml.cs`, `DetailController.ts`, `ScraperDetailStimulusModel.cs`.
- Use the parent Razor Page as the composition shell. It should load independently updateable sections with `<turbo-frame>` elements instead of putting every interaction into one large page.
- Put Turbo Frame sections in nested subfolders by concern, such as `Actions\Actions.cshtml`, `Badges\Badges.cshtml`, `Overview\Overview.cshtml`, `WebPage\WebPage.cshtml`.
- Turbo Frame Razor Pages should usually set `Layout = null`, render a matching `<turbo-frame id="...">`, query only the data needed for that frame, and have their own PageModel and optional colocated TypeScript controller.
- For repeated dynamic sections, use a small partial that emits the `<turbo-frame src="...">` placeholder, then let the frame load its own Razor Page.
- Use Stimulus controllers for client-side behavior, form/job-completion handling, DOM events, and Turbo Frame reloads. Prefer generated C# Stimulus models and generated TypeScript constants over stringly typed data.

## DTO and TypeScript Rules

- Mark DTOs/enums for Reinforced.Typings export and generate TS into existing `Generated` folders.
- Do not manually edit generated `.ts` files; change the C# source or Reinforced.Typings config instead.
- Keep DTOs dumb. Put parsing, normalization, fallback behavior, and URL/path logic in services.

## Testing Rules

- Use TDD for new behavior: write or update the focused test first, then implement.
- Do not add automated tests for CSS. Tests must not parse CSS source, assert individual CSS declarations or cosmetic computed-style values, or lock pixel-perfect presentation. Test user-observable behavior instead. Rendered geometry may be checked only when necessary to prevent functional problems such as clipping, overlap, inaccessible controls, or broken scrolling.
- When adding any testable TypeScript behavior, write the failing test first under `src\Web.ScriptTests\<Feature>\...`, mirroring the feature directory from `src\Web\Features\<Feature>\...` where applicable. Follow the red-green TDD cycle: make the TS/Jest test fail for the expected reason, then implement the smallest change to make it pass.
- Follow test naming: `when_<behavior>.cs`, NUnit, Shouldly, FakeItEasy where useful, builders for setup.
- Put core behavior tests in `src\Core.Tests`; web/page behavior tests in `src\Web.Tests`; browser/E2E behavior in `src\Web.E2E.Tests`.
- Mock backend downloads with fake `HttpMessageHandler`/test clients. Do not hit real backend endpoints in unit tests.
- Before claiming done, run the relevant `dotnet test` project or `dotnet test src\MrWatchdog.sln` when practical.
- If local tests are blocked by a running `MrWatchdog.Web` process locking build outputs, kill that local app process and rerun the tests.
- After implementing browser-visible code, replicate the user scenario in both a desktop browser and BlueStacks Chrome, inspect the result visually in the browser UI, and keep iterating when the rendered behavior or visuals are broken.

## Implementation Plan Rules

- Implementation plans must not put manual browser checks, visual inspection, or human-operated Chrome steps in the required acceptance path when the behavior can be verified automatically.
- For browser-rendered behavior, plans must define automated verification with Playwright, the Codex Browser/Chrome-extension automation, or another repeatable browser automation tool available to the agent. Prefer command-line Playwright tests in `src\Web.E2E.Tests` for checks that should run in CI.
- Chrome extensions and interactive browser sessions may be mentioned only as optional debugging aids. They must not replace automated assertions for routing, rendering, Turbo/Stimulus behavior, media loading, viewport overflow, console errors, or user-visible page content.
- If a browser-rendered requirement cannot be automated, the plan must state the specific technical blocker and the closest automated check that still runs from the agent or CI.

## E2E Testing Rules

- Add E2E coverage in `src\Web.E2E.Tests` for each main web app page type.
- Do not create separate E2E tests for Turbo Frame Razor Pages that are only parts of a main page. Test those frames through the main page that hosts them.
- Use `RunOncePerTestRun.SharedWebApplicationClient` for lightweight E2E tests that only need server-rendered HTTP behavior, such as status codes, redirects, canonical URL handling, headers, cookies, and basic HTML/meta output.
- Use Playwright for browser-rendered E2E tests that must verify the page after JavaScript, Turbo, Stimulus, lazy loading, and browser rendering have run.
- All Playwright E2E tests must launch Chromium with `Headless = true`; the Docker/WSL test environment has no X server available.
- Browser-rendered E2E tests should check the HTTP status code, successful navigation to an URL, main page landmarks/headings, important user-visible data, SEO/link-preview meta tags where relevant, expected Turbo Frames loaded successfully, and no unexpected failed same-origin requests.
- Prefer semantic locators, roles, stable IDs, generated constants, or explicit `data-testid`/`data-e2e` attributes for E2E assertions. Avoid snapshots and selectors that would fail on minor markup or styling refactors.
- Keep Playwright E2E tests focused on user-observable behavior and page composition. Lower-level formatting, cache behavior, and individual Turbo Frame query behavior should stay in unit/integration tests where possible.

## Coding Style

- Match existing C# style: file-scoped namespaces, records for commands/queries, primary constructors where already used, async all the way.
- Preserve each existing file's encoding, byte-order mark, and newline style when editing or moving it. Do not add or remove a BOM or convert between CRLF and LF unless the user explicitly requests it.
- Prefer existing CoreBackend/CoreWeb helpers before adding new infrastructure.
- Keep changes narrow and feature-local. Avoid unrelated refactors and broad formatting churn.
- Keep comments rare; code structure and tests should carry the explanation.
