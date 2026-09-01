# Web.ScriptTests Scaffold Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task. MrWatchdog repository rules require inline execution in the existing `main` checkout and prohibit subagents, worktrees, commits, and branches.

**Goal:** Add an empty Jest/TypeScript project under `src/Web.ScriptTests` for future MrWatchdog TypeScript behavior tests.

**Architecture:** Reuse SmartGuide's standalone JavaScript project structure and generic Jest/jsdom configuration, while excluding every SmartGuide test and app-specific setup file. Register the project in the existing `Tests` solution folder and keep installed npm packages out of Git.

**Tech Stack:** Microsoft Visual Studio JavaScript SDK, TypeScript 5.8, Jest 29, ts-jest 29, jsdom

**Spec:** `docs/superpowers/specs/2026-09-01-web-script-tests-scaffold-design.md`

## Global Constraints

- Work directly in the existing MrWatchdog `main` checkout.
- Do not create commits, branches, worktrees, or subagents.
- The project must contain no feature folders or test files.
- Do not copy SmartGuide's `frontendSettingsTestSetup.js`, `node_modules`, `obj`, `coverage`, or generated artifacts.
- Do not change Docker or CI configuration.

---

### Task 1: Add and integrate the empty ScriptTests harness

**Files:**

- Create: `src/Web.ScriptTests/Web.ScriptTests.esproj`
- Create: `src/Web.ScriptTests/package.json`
- Create: `src/Web.ScriptTests/package-lock.json`
- Create: `src/Web.ScriptTests/jest.config.js`
- Create: `src/Web.ScriptTests/tsconfig.json`
- Create: `src/Web.ScriptTests/styleMock.js`
- Modify: `.gitignore`
- Modify: `src/MrWatchdog.sln`

**Interfaces:**

- Consumes: TypeScript application modules under `src/Web/Features` and npm dependency conventions already used by `src/Web/package.json`.
- Produces: `npm test`, `npm run tsc`, and `npm run verify` commands rooted at `src/Web.ScriptTests`; a Visual Studio solution project named `Web.ScriptTests` under `Tests`.

- [x] **Step 1: Create the project directory and reusable configuration**

Create `src/Web.ScriptTests/Web.ScriptTests.esproj`:

```xml
<Project Sdk="Microsoft.VisualStudio.JavaScript.Sdk/1.0.784122">
  <PropertyGroup>
    <ShouldRunNpmInstall>false</ShouldRunNpmInstall>
    <ShouldRunBuildScript>false</ShouldRunBuildScript>
  </PropertyGroup>
</Project>
```

Create `src/Web.ScriptTests/package.json`:

```json
{
  "version": "1.0.0",
  "name": "web-script-tests",
  "private": true,
  "scripts": {
    "test": "jest",
    "tsc": "tsc --noEmit",
    "verify": "tsc --noEmit && jest"
  },
  "devDependencies": {
    "@types/bootbox": "^5.2.9",
    "@types/hotwired__turbo": "^8.0.4",
    "@types/jest": "^30.0.0",
    "@types/jquery": "^3.5.32",
    "@types/jquery-validation-unobtrusive": "^3.2.35",
    "@types/jquery.validation": "^1.17.0",
    "@types/node": "^24.0.0",
    "jest": "^29.7.0",
    "jest-environment-jsdom": "29.7.0",
    "ts-jest": "^29.1.1",
    "typescript": "^5.8.3"
  }
}
```

Create `src/Web.ScriptTests/jest.config.js` without SmartGuide's frontend-settings setup:

```javascript
module.exports = {
  preset: 'ts-jest',
  testEnvironment: 'jsdom',
  testMatch: ['<rootDir>/**/*.ts'],
  modulePathIgnorePatterns: ['<rootDir>/bin'],
  moduleNameMapper: {
    '\\.(css)$': '<rootDir>/styleMock.js'
  }
};
```

Create `src/Web.ScriptTests/tsconfig.json`:

```json
{
  "compilerOptions": {
    "target": "es2022",
    "module": "commonjs",
    "moduleResolution": "node",
    "strict": true,
    "esModuleInterop": true,
    "allowJs": true,
    "checkJs": false,
    "skipLibCheck": true
  },
  "include": [ "**/*.ts", "jest.config.js" ]
}
```

Including the generic Jest config as unchecked JavaScript gives TypeScript a stable input while the project intentionally contains no `.ts` tests. Future `.ts` files are still checked strictly.

Create `src/Web.ScriptTests/styleMock.js`:

```javascript
module.exports = {};
```

- [x] **Step 2: Generate the dependency lockfile**

Run with `src/Web.ScriptTests` as the working directory:

```powershell
npm install --package-lock-only --ignore-scripts
```

Expected: exit code 0 and `src/Web.ScriptTests/package-lock.json` whose root package is `web-script-tests` and whose lockfile version is 3.

- [x] **Step 3: Add the npm ignore rule**

Append this exact repository-root ignore rule to `.gitignore`:

```gitignore
/src/Web.ScriptTests/node_modules
```

- [x] **Step 4: Register the JavaScript project in the solution**

Add this project block before `Global` in `src/MrWatchdog.sln`:

```text
Project("{54A90642-561A-4BB1-A94E-469ADEE60C69}") = "Web.ScriptTests", "Web.ScriptTests\Web.ScriptTests.esproj", "{554D3678-5715-45CB-80D0-0A517EE10DDB}"
EndProject
```

Add these active configurations to `GlobalSection(ProjectConfigurationPlatforms)`:

```text
		{554D3678-5715-45CB-80D0-0A517EE10DDB}.Debug|Any CPU.ActiveCfg = Debug|Any CPU
		{554D3678-5715-45CB-80D0-0A517EE10DDB}.Debug|x64.ActiveCfg = Debug|Any CPU
		{554D3678-5715-45CB-80D0-0A517EE10DDB}.Debug|x86.ActiveCfg = Debug|Any CPU
		{554D3678-5715-45CB-80D0-0A517EE10DDB}.Release|Any CPU.ActiveCfg = Release|Any CPU
		{554D3678-5715-45CB-80D0-0A517EE10DDB}.Release|x64.ActiveCfg = Release|Any CPU
		{554D3678-5715-45CB-80D0-0A517EE10DDB}.Release|x86.ActiveCfg = Release|Any CPU
```

Add the project to the existing `Tests` solution folder in `GlobalSection(NestedProjects)`:

```text
		{554D3678-5715-45CB-80D0-0A517EE10DDB} = {02EA681E-C7D8-13C7-8484-4AC65E1B71E8}
```

- [x] **Step 5: Install and verify the empty harness**

Run the npm commands with `src/Web.ScriptTests` as the working directory:

```powershell
npm ci --ignore-scripts
npm run tsc
& '.\node_modules\.bin\jest.cmd' --runInBand --passWithNoTests
```

Then run from the repository root:

```powershell
dotnet build src/Web.ScriptTests/Web.ScriptTests.esproj --no-restore
dotnet sln src/MrWatchdog.sln list
```

Expected: every command exits 0, TypeScript emits no files, Jest reports no tests without failing, the JavaScript project builds, and the solution list contains `Web.ScriptTests\Web.ScriptTests.esproj`.

- [x] **Step 6: Inspect the final repository state**

Run:

```powershell
git status --short
git -c core.whitespace=cr-at-eol diff --check
git diff -- .gitignore src/MrWatchdog.sln
git status --short -- src/Web.ScriptTests docs/superpowers
```

Expected: only the approved design/plan records, `.gitignore`, `src/MrWatchdog.sln`, and the six scaffold files are new or modified. `node_modules`, `obj`, test files, and SmartGuide-specific files do not appear.
