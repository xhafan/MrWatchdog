# Web.ScriptTests Scaffold Design

## Goal

Add an empty TypeScript/Jest test project at `src/Web.ScriptTests` so future MrWatchdog TypeScript behavior can follow the TDD location documented in `AGENTS.md`.

## Project contents

Copy the reusable test harness from SmartGuide's current `src/Web.ScriptTests` project:

- `Web.ScriptTests.esproj`
- `package.json` and a matching `package-lock.json`
- `jest.config.js`
- `tsconfig.json`
- `styleMock.js`

The harness will use TypeScript, Jest, ts-jest, and jsdom. Its compiler target and reusable type dependencies will remain aligned with the existing MrWatchdog `src/Web` TypeScript project.

## Exclusions

The new project will contain no tests or feature folders. SmartGuide-specific tests, frontend settings setup, generated build output, installed packages, coverage output, and editor artifacts will not be copied.

## Repository integration

Add `Web.ScriptTests.esproj` to `src/MrWatchdog.sln` under its existing `Tests` solution folder. Add the project's `node_modules` directory to the root `.gitignore`. Docker and CI configuration remain unchanged because executing the future test suite there is outside this scaffolding request.

## Verification

- Install dependencies from the lockfile with `npm ci`.
- Run TypeScript compilation without emitting files.
- Run Jest with its explicit empty-suite option to validate configuration while no tests exist.
- Confirm the solution lists `Web.ScriptTests` and builds the new project successfully.
- Confirm the final Git diff contains only the scaffold, solution integration, ignore rule, lockfile, and this approved design record.
