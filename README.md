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

This repo does not build standalone — cross-project references resolve only inside a consumer workspace. Develop inside a consumer, push changes back with `git subtree push`.
