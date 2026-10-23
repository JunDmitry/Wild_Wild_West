# States

Feature F-004 — Melee attack.

| surface | empty | loading | error | disabled | success | partial |
|---------|-------|---------|-------|----------|---------|---------|
| S-001 melee swing | no swing this step — nothing is shown | — (the swing becomes visible in the same operation that starts the attack; there is no loading state) | — (a refused attack shows no swing and no error) | run Defeat or Victory, or the player defeated — no swing starts; a swing pending when the run ends applies no Damage and shows no hit confirmation | attack starts while melee is equipped and the player is alive — the swing is visible in front of the player through impact, then ends | the swing is visible but damages no enemy — it plays through and ends with no hit confirmation |
| S-001 hit confirmation | the swing damages no enemy — no mark appears anywhere | — (the mark appears at the moment of impact; there is no loading state) | — (no error state; a swing that damages nothing is silent) | run Defeat or Victory — no new confirmation appears | impact damages one or more enemies within reach in front — a mark appears at each damaged enemy and ends with the swing | some enemies in reach are damaged and others are not — marks appear only at the damaged enemies; an enemy that left reach before impact gets none |
| S-001 melee attack input | no attack input this step — no swing starts | an attack is pending — the input starts no new swing and a weapon switch is refused until it resolves | — (no error state; a refused attack is silent and changes nothing) | player defeated, run terminal, the firearm equipped, or the Cooldown not elapsed — the input starts no swing (with the firearm equipped the input performs the firearm attack instead, F-003) | left mouse button while melee is equipped, the player is alive, no attack is pending, and the Cooldown has elapsed — the swing starts | — (a swing is all or nothing; it never starts partly) |

## Notes

- The swing and the hit confirmation are displays, not controls. Their `disabled` state is the terminal run state and the defeated player, not an unavailable control.
- The swing begins with Attack Start and ends with the attack; the hit confirmation exists only at impact and only where Damage was applied. No state persists between attacks.
- A swing that damages nothing still plays through; it simply produces no mark (R7, A7).
- Health never changes for an enemy outside reach, behind the player, or beyond the Attack Arc (BR-116); such an enemy never shows a confirmation.
