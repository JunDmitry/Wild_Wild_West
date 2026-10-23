# States

Feature F-003 — Firearm attack.

| surface | empty | loading | error | disabled | success | partial |
|---------|-------|---------|-------|----------|---------|---------|
| S-001 aiming marker | melee weapon equipped — no marker is shown | — (the marker is drawn with the view; there is no loading state) | — (the marker has no error state) | player defeated or run terminal — the marker is absent | firearm equipped and player alive — the marker runs along the current View Direction and follows the view | — (the marker is one direction; it is never split or partial) |
| S-001 hit confirmation | no shot has damaged an enemy — no confirmation is shown; a miss leaves this state unchanged | — (the confirmation appears in the same operation as the impact; there is no loading state) | — (a miss is not an error and shows no confirmation) | run terminal — no new confirmation appears | a shot damages an enemy — the confirmation appears at that enemy at the moment of impact | — (one shot confirms only the enemy it damaged; the mark is not split across enemies) |
| S-001 firearm attack input | no attack input this step — no shot starts | a shot is pending — the attack input starts no second shot and a weapon switch is refused until it resolves (F-002) | — (a refused or missed shot shows no error state) | melee weapon equipped, player defeated, run terminal, or Cooldown not elapsed — no shot starts | left mouse button while the firearm is equipped, the player is alive, no attack is pending, and the Cooldown has elapsed — a shot starts along the View Direction | — (a shot is wholly one shot; it is never partly started) |

## Notes

- The aiming marker's `empty` and `disabled` values differ: `empty` is the melee weapon being equipped (the marker does not apply), `disabled` is the player being defeated or the run being terminal (the marker is unavailable).
- A missed shot is not an error: it leaves every health value unchanged and draws no confirmation. The marker does not react to a miss.
- The confirmation is transient. Its absence after it fades is the `empty` state, not an error.
- No state exists in which a shot is visible while a confirmation is shown without damage: the confirmation is tied to Damage actually applied.
- The equipped-weapon and health indicators keep their own state rows from F-002 and F-005; F-003 does not change them.
