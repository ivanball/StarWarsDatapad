# Character list and search UI
Labels: feature, ui, estimate: 1h

**Estimate:** 1h
**Depends on:** #3

## Goal
The left-hand list and the header search box from the wireframe.

## Scope
- Load the character list once; filter it in the browser as the user types (case-insensitive, matches any
  part of the name, ignores surrounding whitespace).
- Loading, error (with Retry) and empty states, including "no characters match" for a search with no hits.
- Selecting a character navigates to `/characters/{id}` so a selection can be bookmarked or shared.

## Done when
- bUnit tests cover loading, error and retry, filtering, the empty search result, and selection.
