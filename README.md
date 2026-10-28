# Star Wars Character Datapad

A small application for exploring characters from the Star Wars API ([swapi.info](https://swapi.info/)):
browse and search the characters, select one, and see their attributes, species, homeworld, the films they
appear in, and the starships and vehicles they pilot.

Built with .NET 10: a Blazor WebAssembly front end served by an ASP.NET Core backend-for-frontend (BFF) that
talks to SWAPI.

## See also: v2

This repo is the answer inside the exercise's timebox. A second implementation,
[`../StarWarsDatapad-2`](../StarWarsDatapad-2) (a sibling folder; the GitHub link will be added here when it
is published), keeps exactly the same features and UX and shows how the design scales: explicit layers with
an enforced dependency rule, ADRs for each structural decision, and a test project per layer.

## Run it

Prerequisite: the [.NET 10 SDK](https://dotnet.microsoft.com/download) (10.0.100 or later; `global.json`
rolls forward to the newest 10.0 feature band you have). No database, no API keys, no Node.

```bash
git clone <this repository>
cd StarWarsDatapad
dotnet run --project src/Datapad.Api
```

Open <http://localhost:5080>. One process serves both the API and the compiled Blazor client, on one origin.

Production-style run (what a deployment would do):

```bash
dotnet publish src/Datapad.Api -c Release -o out
cd out && dotnet Datapad.Api.dll --urls http://localhost:5080
```

Note: plain `dotnet run` uses the `http` launch profile, which sets the Development environment. If you run
the build output in Production without publishing (for example `dotnet run --no-launch-profile`), the client's
static files are not mapped and every page returns 404. Publish first, as above.

Configuration (`src/Datapad.Api/appsettings.json`, overridable by environment variables such as
`Swapi__CacheDuration=00:10:00`):

| Setting | Default | Meaning |
| --- | --- | --- |
| `Swapi:BaseAddress` | `https://swapi.info/api/` | Upstream API |
| `Swapi:CacheDuration` | `06:00:00` | How long a loaded SWAPI collection is reused |

## Run the tests

```bash
dotnet test
```

That runs both test projects (about 100 tests, a few seconds, no network: SWAPI is replaced by recorded
fixtures). The runner is Microsoft Testing Platform with xUnit v3 (configured in `global.json`). To run one
project or one test:

```bash
dotnet test --project tests/Datapad.Client.Tests
dotnet test --project tests/Datapad.Api.Tests -- --filter-method "*RelatedCollectionDown*"
```

CI (`.github/workflows/ci.yml`) builds in Release with warnings as errors and runs the same tests on every
push and pull request.

## Architecture

```
Browser (Blazor WebAssembly)                     ASP.NET Core (Datapad.Api)                   swapi.info
+-----------------------------+   GET /api/...   +--------------------------------+   GET    +-----------+
| DatapadPage                 | ---------------> | CharacterEndpoints             |          | /people   |
|  SearchBox                  |                  |   CharacterService (aggregate) |          | /planets  |
|  CharacterList   (filter)   |   JSON contracts |   ISwapiClient                 | -------> | /species  |
|  CharacterDetailsPanel      | <--------------- |     CachedSwapiClient (cache)  |  whole   | /films    |
|   FilmStrip, CraftList, ... |  (Datapad.       |     SwapiClient (HTTP +        |  collec- | /starships|
| IDatapadApi -> HttpClient   |   Contracts)     |       retry/timeout/breaker)   |  tions   | /vehicles |
+-----------------------------+                  +--------------------------------+          +-----------+
```

| Project | Responsibility |
| --- | --- |
| `src/Datapad.Contracts` | The JSON contract shared by server and client (records only, no dependencies). |
| `src/Datapad.Api` | `Swapi/`: everything about the upstream (HTTP, JSON shape, resilience, caching, "unknown" handling). `Characters/`: the two endpoints and the aggregation that shapes data for this page. Also hosts the compiled client. |
| `src/Datapad.Client` | `Services/`: the API client, search filter and display formatting (plain C#, unit tested). `Components/` and `Pages/`: the UI. |
| `tests/Datapad.Api.Tests` | Normalization, HTTP client and caching unit tests; aggregation tests; endpoint tests through `WebApplicationFactory`. |
| `tests/Datapad.Client.Tests` | Filter and formatting unit tests; API client tests; bUnit component tests for every state. |

### Request flow

1. The page loads `GET /api/characters` once (82 names) and filters in the browser as you type.
2. Selecting a character is a link to `/characters/{id}`. The details panel calls `GET /api/characters/{id}`.
3. The API reads the six SWAPI collections it needs (from cache once warm), resolves the character's
   references by URL in memory, normalizes missing values, and returns one response shaped for the page.

### Boundaries worth knowing

- **Only `Swapi/` knows SWAPI exists.** Its records are `internal`; nothing outside maps snake_case fields,
  builds SWAPI URLs or catches `HttpRequestException`. Replacing the upstream means replacing that folder.
- **Only `IDatapadApi` knows the server exists.** Components never touch `HttpClient`; every failure arrives as
  `DatapadApiException` with a message written for the user.
- **The API returns raw but normalized values; the client formats them.** `"unknown"` becomes `null` on the
  server; units, separators and the word "Unknown" are added by `Display` on the client.

## Key decisions and alternatives

**1. A thin backend-for-frontend instead of calling SWAPI from the browser.** swapi.info allows CORS, so a
client-only app would work. I added the BFF because it puts the risky parts (an upstream with no SLA, an
awkward data format, many cross-references) in one testable place with proper resilience and caching, and
gives the UI one small call per selection. The cost is a second deployable and one more hop.
*Alternative:* client-only Blazor WASM with the same `Swapi/` code running in the browser. Simpler to host
(static files only), but every user pays the full load, there is no shared cache, and retries and timeouts
run on the user's connection.

**2. Load whole collections and resolve in memory, instead of fetching each referenced URL.** swapi.info
serves each collection as one unpaginated array; all six together are about 160 KB and never change. Luke
alone references 9 URLs, so the naive approach is 10 or more upstream calls per click. With whole collections
the cold path is 6 calls, and every later request is served from memory (measured locally: 127 ms cold,
about 3 ms warm).
*Tradeoff:* this depends on the dataset being small and static. If the upstream were large, paginated or
changing, I would fetch per resource with a per-URL cache.

**3. Partial failure is a first-class result.** The character itself is required (503 if people cannot be
loaded); each related collection is optional. If, for example, starships fail, the response still returns the
character and lists `"Starships"` in `unavailableSections`. The UI shows an inline notice with Retry in that
section only. *Alternative:* fail the whole request. Simpler, but one flaky collection would blank the page.

**4. Search runs in the browser.** 82 names filter instantly with no request per keystroke, no debounce
and no race between responses. (swapi.info also ignores `?search=`, so it could not be delegated upstream
anyway.) This should move server-side with paging if the list grew to thousands.

**5. The selection lives in the URL** (`/characters/1`), so it can be bookmarked, shared and navigated with
Back and Forward, and the list is plain links (keyboard and screen-reader friendly for free).

**6. Stale responses are dropped.** Each selection cancels the previous request, and a response that arrives
after a newer selection is ignored. Two tests cover this, and both fail if the guard is removed.

**7. Hand-written test doubles, no mocking library.** `FakeSwapiClient` and `FakeDatapadApi` are seeded
from recorded data and switched to failures by setting a property. They read like the scenarios they model.

**Deliberately not done:** no state management library, no UI component library, no CQRS, no repository
layer, no database. The problem is one read-only page over a small static dataset; each of those would add
code to read without reducing risk.

## Assumptions

- **Empty `species` is shown as "Species not recorded in SWAPI", not "Human".** The wireframe shows Luke as
  Human, but swapi.info returns `species: []` for him and 31 other characters, and the Human species record
  lists only four people. Defaulting to Human would make up data for characters whose species is unknown. It
  is a one-line change in `CharacterMapper` if the product owner prefers the wireframe's reading.
- **`"unknown"` and blank values are missing; `"n/a"` and `"none"` are real values.** A droid's gender is
  genuinely not applicable and a bald character's hair colour is genuinely none, so those display as written.
- **SWAPI's placeholder planet** (named `"unknown"`, every field unknown) is shown as "Homeworld unknown."
  rather than as a card full of blanks.
- **All six films are shown, with the character's films highlighted**, because the wireframe dims Episodes I
  and II for Luke rather than hiding them.
- **Values SWAPI writes as words or ranges** (`"indefinite"`, `"30-165"`) are shown as written, without a
  unit; only clean numbers get thousands separators and units (`"10465"` -> `10,465 km`).
- **Characters are sorted alphabetically** (the wireframe list is alphabetical; SWAPI's order is by id).
- **Desktop first**, matching the wireframe; the layout collapses to one column below 760 px but has not had
  a dedicated mobile design pass.

## Testing strategy

Tests are concentrated where a bug would be most likely or most costly:

| Risk | Where it is tested |
| --- | --- |
| SWAPI's data quirks (unknown, n/a, commas, trailing slashes, dangling references, the placeholder planet) | `SwapiValueTests`, `CharacterServiceTests` (against fixtures recorded from swapi.info) |
| Upstream failures: error status, timeout, bad JSON, network down, and caller cancellation not mistaken for an outage | `SwapiClientTests` |
| A failure must never be cached | `CachedSwapiClientTests` |
| Partial failure, 404, 503, problem details, unknown `/api` routes not returning the HTML page | `CharacterServiceTests`, `CharacterEndpointsTests` |
| DI wiring of the real typed client, resilience handler and cache (6 upstream calls, then a cache hit) | `CharacterEndpointsTests.GetCharacter_ThroughRealHttpPipeline_...` |
| Every UI state: loading, empty, error with Retry, not found without Retry, unavailable section, out-of-order responses | `CharacterListTests`, `CharacterDetailsPanelTests` |
| Formatting and search rules | `DisplayTests`, `CharacterFilterTests` |

Not covered by automated tests: a real-browser end-to-end run (issue #7) and visual layout. I checked those
by hand against the live API: the wireframe layout, search, and the details for Luke.

## Risks and limitations

- **swapi.info is a free community mirror with no SLA.** Resilience (timeouts, retries, circuit breaker) and
  caching soften it, but a cold start during an outage still shows the error state. Issue #8 (warm the cache
  at startup, serve the last good copy) is the fix.
- **The cache is per process and in memory.** Fine for one instance; several instances would each load SWAPI.
  A distributed cache (or `HybridCache`) is the next step.
- **Concurrent cold requests can each go upstream once** (no stampede protection). Acceptable at six small
  collections; `HybridCache` would also solve this.
- **The upstream contract is not versioned.** If swapi.info changes its JSON, deserialization fails loudly
  (a 503 with a logged `JsonException`) rather than rendering wrong data. Fixture tests pin the shape we
  depend on.
- **The whole-collection approach** (decision 2) is only valid while the dataset stays small.
- **No accessibility audit yet** (issue #9). The markup uses landmarks, real links, table headers, labels and
  `role=status` / `role=alert`, but it has not been tested with a screen reader or axe.

## What I would do next, in order

1. **Warm the cache at startup and serve stale data on failure** (#8): removes the cold-start outage risk.
2. **One Playwright end-to-end test** (#7): catches hosting and routing wiring that unit tests cannot.
3. **Accessibility pass** (#9): arrow-key navigation in the list, axe audit, contrast of the dimmed films.
4. **Container and deployment** (#10), plus OpenTelemetry on the upstream calls to see latency and failures.
5. **Product questions to settle:** the species assumption above, and whether the films strip should link
   somewhere.

## Working in this codebase

- **Adding a field to the page:** add it to the SWAPI record in `Swapi/SwapiModels.cs` (snake_case is mapped
  automatically), to the contract in `Datapad.Contracts/Characters.cs`, map it through `SwapiValue.Normalize`
  in `CharacterMapper`, then render it with a `Display` helper. The compiler walks you through the chain.
- **Adding a related section** (for example the character's homeworld residents): add the collection to
  `ISwapiClient` and both implementations, add a `DetailSection` value, resolve it in `CharacterService` with
  `TryLoadAsync` so it degrades on failure, and give the component a `SectionUnavailable` branch.
- **Never let SWAPI types leave `Swapi/`** and never call `HttpClient` from a component; those two rules keep
  the upstream swappable and the UI testable.
- **Test data:** `tests/Datapad.Api.Tests/Fixtures/*.json` are recorded swapi.info responses trimmed to Luke,
  R2-D2 and Arvel Crynyd (chosen because they cover a full record, a droid, and an all-unknown record). Add a
  record there when you need a new edge case, rather than inventing JSON in a test.
- **Conventions:** nullable on and warnings as errors (`Directory.Build.props`), package versions only in
  `Directory.Packages.props`, `.editorconfig` for style. A warning fails the build, locally and in CI.

## Work tracking

The work was planned as GitHub issues before coding. The issue bodies live in `docs/issues/` (with estimates,
dependencies and risks); `scripts/create-issues.ps1` creates them on a fresh repository in order, so the
`#N` references in the commits line up. Each issue was built on its own branch and merged with `--no-ff`,
so `git log --graph` shows one merge per issue.

| # | Issue | Estimate | Status |
| --- | --- | --- | --- |
| 1 | Scaffold solution, conventions and CI | 0.5h | Done |
| 2 | SWAPI gateway: typed client, resilience, caching, normalization | 1.5h | Done |
| 3 | Character endpoints: list and aggregated details | 1h | Done |
| 4 | Character list and search UI | 1h | Done |
| 5 | Character details UI | 1.5h | Done |
| 6 | README | 0.5h | Done |
| 7 | Backlog: end-to-end smoke test | 1h | Not started |
| 8 | Backlog: cache warm-up and stale-on-failure | 1h | Not started |
| 9 | Backlog: accessibility pass | 1h | Not started |
| 10 | Backlog: container and deployment | 1.5h | Not started |

## How AI tools were used

I built this with Claude Code, an AI coding agent working in my local repository. I set the direction and
the agent did most of the typing; I remain responsible for every line, so this is how the work was split and
how I checked it.

- **What I did:** gave the agent the exercise and the brief, approved the plan and the issue breakdown before
  any code was written, read the code and the diffs as the issues landed, and ran the app and the tests on my
  own machine.
- **What the agent produced:** the issue breakdown and estimates, the code, the tests and the first draft of
  this README, one issue at a time, each on its own branch merged into `main`.
- **What decided the design:** before any code, the agent probed swapi.info and confirmed that it serves
  whole unpaginated collections, ignores `?search=`, allows CORS, has gaps in its ids and encodes missing data
  as strings ("unknown", "n/a"). Those observed facts, not the model's defaults, drove the main design points:
  caching whole collections, searching in the browser, taking identity from the `url` field, and the
  normalization rules.
- **How the output was checked:**
  - Warnings as errors plus the .NET analyzers on every build.
  - Tests run against fixtures recorded from the real API, not invented JSON. Two of the first test runs
    failed because the expectations were wrong about the data ("Imperial Speeder Bike", the Droid language
    "n/a"). The tests were corrected to match the recorded data, not the other way round.
  - A mutation check on the riskiest UI logic: removing the stale-response guard makes both race tests fail.
  - Running the app against the live API and checking the rendered page against the wireframe (layout,
    search, Luke's details, the not-found and no-match states). That run caught a real hosting issue (static
    files are not served when the build output runs in Production without publishing), now documented above.
  - A published Release build run end to end.
