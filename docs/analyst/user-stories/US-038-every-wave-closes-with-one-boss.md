# US-038 — Every wave closes with one boss

- **Feature:** F-009 — Boss enemy each wave
- **Actor:** player
- **Priority:** P1 (from `docs/product/backlog.md`)
- **Rules:** `BR-053`, `BR-056`, `BR-060`, `BR-061`, `BR-062`

## Story

As a player, I want exactly one boss to close each of the three waves, so that
every wave has a single decisive fight and none is skipped, repeated, or
doubled.

## Examples

**E1 — each wave ends with a boss**
- Given any Wave of the run,
- when its Regular Combat ends,
- then exactly one Boss enters to close that Wave.

**E2 — no second boss joins**
- Given a Wave's Boss is Active or already defeated,
- when that Wave continues,
- then no second Boss becomes active in it.

**E3 — a wave with no regular enemies still has its boss**
- Given a Wave with no Regular Enemies,
- when it begins,
- then it begins in Boss Combat and its Boss closes it.

**E4 — the third boss ends the run**
- Given the final Wave's Boss is defeated,
- when the Wave completes,
- then the ArenaRun reaches Victory.

## Notes

A run comprises three Waves and the third is the final Wave; defeating its boss
is the run's last required act. Multiple bosses per wave, additional boss types,
and boss music or arena changes are out of scope.
