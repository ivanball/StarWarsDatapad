# Scaffold solution, conventions and CI
Labels: chore, estimate: 0.5h

**Estimate:** 0.5h

## Goal
A buildable, testable skeleton that every later issue lands into.

## Scope
- `StarWarsDatapad.slnx` with `src/` (Api, Client, Contracts) and `tests/` (Api.Tests, Client.Tests).
- .NET 10, nullable on, warnings as errors, central package management, `.editorconfig`.
- The API hosts the compiled Blazor WebAssembly client (one process, one origin, no CORS).
- GitHub Actions workflow: restore, build, test on every push and PR.
- One smoke test per test project so the pipeline proves discovery works.

## Done when
- `dotnet build` and `dotnet test` pass locally and in CI.
