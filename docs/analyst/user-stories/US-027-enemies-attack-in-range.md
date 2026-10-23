# US-027 — Enemies attack in range

- **Feature:** F-007 — Enemy pursuit and close-range attack
- **Actor:** player
- **Priority:** P1 (from `docs/product/backlog.md`)
- **Rules:** `BR-044`, `BR-099`

## Story

As a player, I want an enemy to attack me once it is within reach, so that
letting an enemy close the distance has a cost and the threat is real.

## Examples

**E1 — an enemy in range attacks instead of closing further**
- Given an Enemy whose Distance to the Player is within its Attack Range,
- when the Enemy is ready to attack,
- then the Enemy starts an Enemy Attack rather than moving further toward
  the Player.

**E2 — an enemy without a ready attack keeps its distance**
- Given an Enemy in range with a pending Enemy Attack,
- when Game Time advances,
- then no second Enemy Attack starts until that one is resolved and the
  Enemy is ready again.

**E3 — the player steps out and back into range**
- Given an Enemy that attacked the Player and is now out of range,
- when the Player moves away and then returns within Attack Range,
- then the Enemy resumes pursuit and attacks again once in range and ready.

## Notes

Attack range, damage, attack rate, and movement speed are tuning values.
Attack timing remains a tuning value; the ordering — an attack's cue is
visible before its damage lands — is fixed by `BR-120` and told in `US-044`.
