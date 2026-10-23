# US-046 — The defeat message names the wave reached

- **Feature:** F-010 — Defeat and scene restart
- **Actor:** player
- **Priority:** P1 (from `docs/product/backlog.md`)
- **Rules:** `BR-051`, `BR-122`

## Story

As a player, I want the defeat message to name the wave I had reached, so
that I know how far I got before losing.

## Examples

**E1 — a defeat in a later wave**
- Given the run ended in Defeat while the second Wave was current,
- when the defeat message is shown,
- then it names the second Wave.

**E2 — a defeat in the first wave**
- Given the run ended in Defeat while the first Wave was current,
- when the defeat message is shown,
- then it names the first Wave.

**E3 — two defeats name their own waves**
- Given two runs that ended in Defeat during different Waves,
- when each defeat message is shown,
- then each names the Wave that was current when its run ended, not the
  other's.

## Notes

The named wave is the Wave that was current at the moment of Defeat.
