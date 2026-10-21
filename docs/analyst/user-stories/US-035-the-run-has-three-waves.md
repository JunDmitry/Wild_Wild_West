# US-035 — The run has three waves

- **Feature:** F-008 — Wave progression
- **Actor:** player
- **Priority:** P1 (from `docs/product/backlog.md`)
- **Rules:** `BR-060`, `BR-061`, `BR-012`

## Story

As a player, I want the run to be three waves long with a clearly marked final
one, so that the fight builds to a definite end instead of dragging on with no
sense of how close I am to finishing.

## Examples

**E1 — the third wave is the final one**
- Given the second Wave is complete,
- when the third Wave begins,
- then that Wave is the final Wave.

**E2 — completing the final wave ends the run**
- Given the final Wave is complete,
- when the Wave completes,
- then the ArenaRun reaches Victory and no fourth Wave begins.

**E3 — the run's end is terminal**
- Given the ArenaRun is victorious,
- when Game Time advances,
- then no new Wave begins and no Enemy enters the Arena.

## Notes

Victory is terminal: a new fight is a new ArenaRun, never a fourth wave of this
one. Endless mode and inter-wave menus or countdowns are out of scope.
