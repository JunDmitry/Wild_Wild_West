# US-036 — The boss arrives when the regular enemies fall

- **Feature:** F-009 — Boss enemy each wave
- **Actor:** player
- **Priority:** P1 (from `docs/product/backlog.md`)
- **Rules:** `BR-054`, `BR-055`, `BR-071`, `BR-072`, `BR-073`, `BR-048`

## Story

As a player, I want the boss to enter only after the wave's regular enemies are
defeated, so that each wave builds to a single decisive fight instead of opening
with its hardest enemy.

## Examples

**E1 — no boss while regular enemies remain**
- Given the current Wave is in Regular Combat with Regular Enemies still to
  spawn or still Active,
- when Game Time advances,
- then no Boss enters the Arena.

**E2 — the last regular enemy falls**
- Given the current Wave's last Regular Enemy is defeated,
- when the Wave moves to Boss Combat,
- then a Boss enters the Arena from outside Arena Bounds.

**E3 — the boss is seen only after it enters**
- Given a Boss is due in Boss Combat,
- when it enters,
- then it becomes visible only after coming in from outside Arena Bounds.

## Notes

The boss's exact health, size, and attack values are tuning values.
Boss-specific attacks and phases, more than one boss type, and more than one
boss per wave are out of scope.
