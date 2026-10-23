# Screens

Feature F-003 — Firearm attack.

| id | screen | one job | flows that reach it |
|----|--------|---------|---------------------|
| S-001 | Arena gameplay view | show the player character in third person inside the arena and reflect movement, view, equipped weapon, health, enemy entry, the firearm's aim, and a landed shot | FL-024, FL-025, FL-026, FL-027, FL-028 |

## Notes

- S-001 is the screen F-001 designed, F-002 extended with the equipped-weapon indicator, F-005 with the health indicator, and F-006 with enemy entry. F-003 extends it with an aiming marker and a hit confirmation; it mints no new screen id.
- The aiming marker and the hit confirmation are in-world parts of the view, not overlays or HUD regions. No flow can reach either on its own.
- The marker is a persistent direction indication while the firearm is equipped and the player is alive. The confirmation is transient and appears only when a shot damages an enemy.
- The equipped-weapon and health indicators stay in their fixed corners and are unchanged. F-003 adds no counter, no ammunition display, and no reload surface.
- The screen's single job grows by one clause: it now also shows where a shot would travel and confirms a landed shot. The movement, view, weapon, health, and enemy-entry clauses are unchanged.
