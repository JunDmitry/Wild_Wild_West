# States

Feature F-002 — Weapon switching.

| surface | empty | loading | error | disabled | success | partial |
|---------|-------|---------|-------|----------|---------|---------|
| S-001 equipped-weapon indicator | — (exactly one weapon is equipped whenever the view is shown) | — (equipping is instantaneous; there is no loading state) | — (a refused switch leaves the indicator unchanged; no error state is shown) | run Defeat or Victory — the indicator keeps showing the weapon held at the end and does not change | run Playing — the indicator shows the equipped weapon and updates on each successful switch | — (a switch is never partially applied; exactly one weapon is shown) |
| S-001 weapon-switch input | no switch input this step — the equipped weapon is unchanged | — (the switch completes in the same step; there is no loading state) | — (there is no error state; a refused switch is silent and leaves the weapon unchanged) | player defeated, or a pending attack exists — Q produces no switch and the indicator does not change | Q while the player is alive with no pending attack — the other weapon becomes equipped and the indicator updates | — (the switch cannot be partly applied; one weapon is equipped before and after) |
| S-001 attack input | no attack input this step — no attack starts | an attack is pending — the attack input does not start another attack, and a weapon switch is refused until it resolves | — (attack outcomes are F-003 and F-004; F-002 shows no error state) | player defeated — the attack input starts no attack | left mouse button while the player is alive and ready — the equipped weapon's attack starts (firearm attack or melee attack) | — (the attack is wholly one weapon's; both weapons never act together) |
