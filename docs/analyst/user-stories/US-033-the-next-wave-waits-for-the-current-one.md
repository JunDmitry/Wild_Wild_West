# US-033 — The next wave waits for the current one

- **Feature:** F-008 — Wave progression
- **Actor:** player
- **Priority:** P1 (from `docs/product/backlog.md`)
- **Rules:** `BR-052`, `BR-059`, `BR-054`, `BR-062`

## Story

As a player, I want the next wave to begin only after I have cleared the
current one, so that I never face two waves at once and each engagement is a
complete fight rather than a pile-up.

## Examples

**E1 — an unfinished wave blocks the next**
- Given the current Wave is not complete,
- when Enemies are defeated and others remain,
- then no new Wave begins.

**E2 — completing the wave starts the next**
- Given the current Wave is complete and it is not the final Wave,
- when the Wave completes,
- then the next Wave begins and becomes the current Wave.

**E3 — a new wave starts at its first phase**
- Given a non-final Wave completes,
- when the next Wave begins,
- then it begins in Regular Combat, or in Boss Combat if it has no Regular
  Enemies.

## Notes

Every Wave runs Regular Combat then Boss Combat before it can complete, so the
sequencing above is the only path from one Wave to the next.
