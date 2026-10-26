# SWAPI gateway: typed client, resilience, caching and data normalization
Labels: feature, api, estimate: 1.5h

**Estimate:** 1.5h

## Goal
Isolate everything about talking to `https://swapi.info/api` behind one interface, so no other code knows
the upstream URL shape, JSON field names, or failure modes.

## Findings that shape the design (probed 2026-10-26)
- Every collection endpoint (`/people`, `/films`, `/planets`, `/species`, `/starships`, `/vehicles`) returns
  the whole collection as one unpaginated JSON array. The full dataset is roughly 160 KB.
- The data is static (the films do not change), so caching for hours is safe.
- `?search=` is ignored by swapi.info (it returns everything), so search cannot be delegated upstream.
- IDs have gaps (`/people/17` is a 404), so identity must come from the `url` field, not array position.
- Missing data is encoded as strings: `"unknown"`, `""`; numbers can contain commas (`"1,358"`) or ranges
  (`"30-165"`); `"n/a"` and `"none"` are real values (a droid's gender, a bald character's hair).
- 32 of 82 people have `species: []` (including Luke, whom the wireframe shows as Human).

## Scope
- `ISwapiClient` + typed `HttpClient` implementation reading whole collections.
- Standard resilience handler (timeout, retry with backoff on transient errors, circuit breaker).
- In-memory cache per collection with a long TTL; failures are not cached.
- Mapping that turns `"unknown"` and empty strings into `null`, keeping meaningful values as-is.

## Risks
- Upstream is a free community mirror with no SLA: it can be slow, down, or change shape.
- Treating `"n/a"` as missing would hide real information; treating `"unknown"` as a value would render
  as if it were data.

## Done when
- Unit tests cover deserialization against recorded fixtures, normalization rules, caching, and error mapping.
