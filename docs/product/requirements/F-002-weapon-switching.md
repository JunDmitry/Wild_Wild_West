# F-002 — Weapon switching

- **Priority:** P1
- **State:** specified
- **Backlog:** `docs/product/backlog.md`
- **Handoff:** `docs/analyst/product-inbox/F-002.md`, `docs/design/product-inbox/F-002.md`

## Problem

The player carries a firearm and a melee weapon, and the brief requires moving between them during combat. Without a switch the second weapon is unreachable and the attack input has no defined meaning.

## Requirements

- **R1** — pressing Q switches from the firearm to the melee weapon, and pressing Q again switches back to the firearm.
- **R2** — exactly one weapon is equipped at any moment.
- **R3** — every run starts with the firearm equipped.
- **R4** — after a switch, the attack input performs only the equipped weapon's attack.
- **R5** — switching is available whenever the player is alive.
- **R6** — a dead player cannot switch weapons.
- **R7** — each weapon's attack readiness is its own: a switch neither transfers, resets, nor shares the other weapon's pending attack or cooldown.

## Acceptance criteria

- **A1** — with the firearm equipped, one press of Q equips the melee weapon; the next press re-equips the firearm.
- **A2** — with melee equipped, the attack input performs the melee attack; with the firearm equipped, it performs the firearm attack.
- **A3** — at the start of every run, the firearm is equipped.
- **A4** — no input other than Q changes the equipped weapon.
- **A5** — while the player is dead, Q does not change the equipped weapon.
- **A6** — attack with the firearm, switch to melee and back before its cooldown elapses; the firearm's attack is still unavailable when the player returns, for the remainder of its original interval.

## Out of scope

- More than two weapons, weapon pickups, or loadouts.
- Dual wielding or simultaneous use of both weapons.
- Switching animation timing, holstering rules, or per-weapon movement penalties.

## Revisions

- 2026-10-23 — R7 and A6 added. The rules did not say whether a switch keeps
  the other weapon's pending attack or cooldown
  (`docs/architect/tasks/F-002.md` § Open questions), and the behaviour was
  being settled in the implementation. Each weapon's readiness is now
  independent and stated. Superseding request:
  `docs/analyst/product-inbox/F-002b.md`.
