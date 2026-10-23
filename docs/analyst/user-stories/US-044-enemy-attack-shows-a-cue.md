# US-044 — An enemy attack shows a cue before it lands

- **Feature:** F-007 — Enemy pursuit and close-range attack
- **Actor:** player
- **Priority:** P1 (from `docs/product/backlog.md`)
- **Rules:** `BR-092`, `BR-099`, `BR-120`

## Story

As a player, I want to see an enemy's attack coming before its damage lands,
so that I can read the threat and react instead of taking damage with no
warning.

## Examples

**E1 — the cue comes first**
- Given an enemy is within its attack range and ready to attack,
- when the enemy starts an attack,
- then a visible cue appears,
- and the player's health is unchanged at that moment,
- and only after the cue has been visible does the attack's damage land.

**E2 — a cancelled attack shows no damage**
- Given an enemy has started an attack and its cue is visible,
- when the enemy is defeated before the attack lands,
- then no damage is applied to the player.

**E3 — the cue's length varies, the order does not**
- Given two enemy attacks whose cues have different durations,
- when each attack resolves,
- then in both cases the cue is visible before that attack's damage is
  applied.

## Notes

The cue's duration is a tuning value; the ordering — cue visible before
damage — is fixed. The cue's visual form is not defined here.
