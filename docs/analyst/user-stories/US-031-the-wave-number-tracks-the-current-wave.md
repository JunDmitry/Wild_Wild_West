# US-031 — The wave number tracks the current wave

- **Feature:** F-008 — Wave progression
- **Actor:** player
- **Priority:** P1 (from `docs/product/backlog.md`)
- **Rules:** `BR-051`, `BR-050`, `BR-064`

## Story

As a player, I want the run to keep an ordered count of which wave I am on, so
that clearing a wave always moves me to a later, well-defined stage of the run
rather than leaving me unsure where I stand.

## Examples

**E1 — a run starts on the first wave**
- Given a new ArenaRun,
- when it starts Playing,
- then the current Wave's Wave Number is one.

**E2 — the number advances with the new wave**
- Given the current Wave is complete and it is not the final Wave,
- when the next Wave begins,
- then that Wave's Wave Number is one greater than the previous Wave's.

**E3 — the number never advances early**
- Given the current Wave is still in progress with Enemies remaining,
- when some of them are defeated,
- then the current Wave's Wave Number is unchanged.

## Notes

The count is run state, not a display: a visible wave-number display is out of
scope. Exactly one Wave is current at a time, so there is only ever one Wave
Number to track.
