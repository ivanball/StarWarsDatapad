# Character details UI: attributes, species, homeworld, films, starships, vehicles
Labels: feature, ui, estimate: 1.5h

**Estimate:** 1.5h
**Depends on:** #3, #4

## Goal
The details panel and the three related sections from the wireframe, with image placeholders.

## Scope
- Attributes table, species card, homeworld card.
- Films strip showing all films in episode order, dimming the ones the character is not in (as in the
  wireframe).
- Starships and vehicles lists with class, crew / passengers, model / manufacturer.
- Missing values render as "Unknown"; empty relations render an explicit "none" message; sections the API
  reports as unavailable render an inline notice while the rest of the page still works.
- Selecting characters quickly must never show a slower, older response over the newer one.

## Done when
- bUnit tests cover the populated view, missing data, empty relations, an unavailable section, a failed
  load with retry, and the out-of-order response race.
