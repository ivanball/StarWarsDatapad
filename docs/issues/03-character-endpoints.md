# Character endpoints: list and aggregated details
Labels: feature, api, estimate: 1h

**Estimate:** 1h
**Depends on:** #2

## Goal
A backend-for-frontend shaped for the one page: the client makes one call for the list and one call per
selected character, and never stitches SWAPI URLs together itself.

## Scope
- `GET /api/characters` returns `{ id, name }` for every character, sorted by name.
- `GET /api/characters/{id}` returns attributes, species, homeworld, all films (flagged when the
  character appears), starships and vehicles.
- Partial failure: if a related collection cannot be loaded, the character still renders and the response
  names the unavailable sections, instead of failing the whole request.
- Errors as RFC 7807 problem details: 404 for an unknown character, 503 when SWAPI itself is unreachable.

## Risks
- A naive implementation fans out 10 or more upstream calls per click. Resolving from cached collections
  removes that.

## Done when
- Integration tests through `WebApplicationFactory` with a fake upstream cover 200, 404, 503 and partial
  failure.
