# US-042 — The victory window announces the win

- **Feature:** F-011 — Victory after three waves
- **Actor:** player
- **Priority:** P1 (from `docs/product/backlog.md`)
- **Rules:** `BR-118`, `BR-061`, `BR-057`, `BR-013`, `BR-015`

## Story

As a player, I want a Victory window to tell me the run is won the moment the
third wave's boss falls, so that finishing the fight feels like a win instead of
the arena simply going quiet.

## Examples

**E1 — the third boss falls and the run is won**
- Given the final Wave's Boss is the last Active Enemy of the third Wave,
- when that Boss is defeated,
- then the Wave completes, the ArenaRun reaches Victory, and a Victory window
  stating that the run was won appears.

**E2 — no window before the final boss falls**
- Given the first or second Wave's Boss has just been defeated,
- when that Wave completes,
- then the ArenaRun continues in the next Wave and no Victory window appears.

**E3 — surviving enemies keep the win away**
- Given the third Wave still has Active Enemies,
- when some of those Enemies are defeated,
- then no Victory window appears.

**E4 — dying instead of winning**
- Given the third Wave's Boss is still Active,
- when the player's Health reaches zero before that Boss is defeated,
- then the ArenaRun is in Defeat and no Victory window appears.

## Notes

The window states that the run has been won. Victory menu actions (restart,
quit, continue), score displays, rewards, unlocks, leaderboards, and victory
animation, music, or camera work are out of scope. How the window looks and
where it sits is `design-role`'s decision; this story fixes only that it appears
and what it says.
