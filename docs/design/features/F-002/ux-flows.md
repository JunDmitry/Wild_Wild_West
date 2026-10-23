# UX Flows

Feature F-002 — Weapon switching.

### FL-004 — Weapon switch while alive

Primary flow. Complete.

```mermaid
flowchart TD
  A(["run starts — Playing; firearm equipped"]) --> B["S-001 — Arena gameplay view; indicator shows Firearm"]
  B --> C{"Q pressed?"}
  C -- "no" --> B
  C -- "yes" --> D{"player alive and no pending attack?"}
  D -- "no" --> E["S-001 — switch refused; equipped weapon unchanged"]
  E --> B
  D -- "yes" --> F["S-001 — the other weapon becomes equipped; indicator updates"]
  F --> B
  B --> G{"run reaches Defeat or Victory?"}
  G -- "no" --> B
  G -- "yes" --> H(["switching no longer changes the equipped weapon"])
```

### FL-005 — Weapon switch refused while dead

Edge flow. Complete.

```mermaid
flowchart TD
  A(["player is defeated; Q pressed"]) --> B["S-001 — equipped weapon unchanged; indicator keeps the weapon held at defeat"]
  B --> C(["no switch; the run remains terminal"])
```

### FL-006 — Weapon switch refused during a pending attack

Edge flow. Complete.

```mermaid
flowchart TD
  A(["player alive; a pending attack exists; Q pressed"]) --> B{"does a pending attack still exist?"}
  B -- "yes" --> C["S-001 — switch refused; equipped weapon unchanged"]
  C --> D{"pending attack resolved?"}
  D -- "no" --> C
  D -- "yes" --> E["S-001 — a later Q press switches the weapon; indicator updates"]
  B -- "no" --> E
```

### FL-007 — A new run starts with the firearm

Edge flow. Complete.

```mermaid
flowchart TD
  A(["new run begins — first run or automatic restart"]) --> B["S-001 — firearm equipped; indicator shows Firearm"]
  B --> C(["the previous run's equipped weapon does not carry over"])
```

### FL-008 — Attack input follows the equipped weapon

Edge flow. Complete.

```mermaid
flowchart TD
  A(["attack input pressed — left mouse button"]) --> B{"player alive?"}
  B -- "no" --> C["S-001 — no attack starts"]
  B -- "yes" --> D{"which weapon is equipped?"}
  D -- "firearm" --> E["S-001 — firearm attack starts (F-003)"]
  D -- "melee weapon" --> F["S-001 — melee attack starts (F-004)"]
```

## Notes

- F-002 routes the attack input; whether the attack itself starts, and its timing, are F-003 and F-004.
- The equipped-weapon indicator is part of S-001, not a screen of its own.

## Open questions

- None. The spec and analysis settle F-002's behaviour, and the persistent equipped-weapon indicator was selected by the user on 2026-10-23.
