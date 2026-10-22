# US-043 — The run stops when it is won

- **Feature:** F-011 — Victory after three waves
- **Actor:** player
- **Priority:** P1 (from `docs/product/backlog.md`)
- **Rules:** `BR-117`, `BR-012`, `BR-014`, `BR-031`, `BR-075`

## Story

As a player, I want the arena to stop once I have won, so that the victory I
earned cannot be undone by a stray attack or a late-arriving enemy.

## Examples

**E1 — no attack after the win**
- Given the ArenaRun is Victory,
- when the player provides Attack Input,
- then no Attack starts and no Enemy takes Damage.

**E2 — no movement after the win**
- Given the ArenaRun is Victory,
- when the player provides Movement Input,
- then the player's Position does not change.

**E3 — nothing more arrives after the win**
- Given the ArenaRun is Victory,
- when Game Time advances,
- then no Enemy enters the Arena and no new Wave begins.

**E4 — the win is terminal**
- Given the ArenaRun is Victory,
- when Game Time advances,
- then the ArenaRun never returns to Playing and never reaches Defeat.

## Notes

Victory is terminal: a new fight is a new ArenaRun, never a continuation of a
won one. Victory menu actions (restart, quit, continue) and score displays are
out of scope.
