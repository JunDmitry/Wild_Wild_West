# UX Flows

Feature F-004 — Melee attack.

### FL-029 — Melee swing that damages enemies in front

Primary flow. Complete.

```mermaid
flowchart TD
  A(["run Playing; player alive; melee weapon equipped; no pending attack; melee Cooldown elapsed"]) --> B["S-001 — attack input: the swing becomes visible in front of the player"]
  B --> C{"at impact, is an enemy within reach in front of the player?"}
  C -- "yes" --> D["S-001 — every such enemy loses health once"]
  D --> E["S-001 — a hit confirmation appears at each damaged enemy at the moment of impact"]
  E --> F["S-001 — the swing ends; the melee weapon becomes ready when its Cooldown elapses"]
  C -- "no" --> G["S-001 — no hit confirmation; every health value is unchanged"]
  G --> F
  F --> H{"does the run remain Playing?"}
  H -- "yes" --> A
  H -- "no — Defeat or Victory" --> I(["no further swing starts"])
```

### FL-030 — A swing that damages nothing

Edge flow. Complete.

```mermaid
flowchart TD
  A(["a swing resolves at impact"]) --> B{"is an enemy within reach in front of the player?"}
  B -- "yes" --> C["S-001 — each such enemy takes damage once; a hit confirmation appears at each"]
  B -- "no" --> D["S-001 — every health value is unchanged"]
  D --> E["S-001 — no hit confirmation appears; the swing still plays through and ends"]
```

### FL-031 — Melee attack refused by readiness

Edge flow. Complete.

```mermaid
flowchart TD
  A(["melee weapon equipped; player alive; left mouse button pressed"]) --> B{"does a pending attack exist, or has the Cooldown not elapsed?"}
  B -- "yes" --> C["S-001 — no swing starts; the pending attack continues to its impact"]
  C --> D{"has the Cooldown elapsed since that attack started?"}
  D -- "no" --> C
  D -- "yes" --> E["S-001 — a later press starts a swing"]
  B -- "no" --> E
```

### FL-032 — Attack input with the firearm equipped or the player defeated

Edge flow. Complete.

```mermaid
flowchart TD
  A(["left mouse button pressed"]) --> B{"is the player alive?"}
  B -- "no" --> C["S-001 — no swing starts; the attack input changes nothing"]
  B -- "yes" --> D{"is the melee weapon equipped?"}
  D -- "no — firearm equipped" --> E["S-001 — no swing starts; the firearm attack is F-003's"]
  D -- "yes" --> F["S-001 — the melee attack is available (FL-029)"]
```

### FL-033 — Several enemies within reach in front

Edge flow. Complete.

```mermaid
flowchart TD
  A(["a swing resolves at impact"]) --> B{"how many enemies lie within reach in front of the player?"}
  B -- "none" --> C["S-001 — no health changes; no hit confirmation"]
  B -- "one or more" --> D["S-001 — each of them loses health once"]
  D --> E["S-001 — a hit confirmation appears at each damaged enemy"]
  E --> F["S-001 — the swing ends"]
```

### FL-034 — An enemy leaves reach before impact

Edge flow. Complete.

```mermaid
flowchart TD
  A(["an enemy was within reach in front at Attack Start"]) --> B["S-001 — the swing is visible in front of the player"]
  B --> C{"is that enemy still within reach in front at impact?"}
  C -- "yes" --> D["S-001 — the enemy is damaged and confirmed (FL-029)"]
  C -- "no" --> E["S-001 — that enemy's health is unchanged; no confirmation appears at it"]
  E --> F{"are any other enemies within reach in front at impact?"}
  F -- "yes" --> G["S-001 — those enemies are damaged and confirmed"]
  F -- "no" --> H["S-001 — the swing ends with no hit confirmation"]
```

## Notes

- The swing is the melee attack's visible form: in-world, in front of the player, and transient. It is the only surface R7 asks for; no standing reach indicator or HUD control is added.
- The hit confirmation is tied to Damage actually applied. One appears at each enemy that lost health (R8, A7); a swing that damages nothing produces none.
- Attack Start applies no Damage, and Impact resolves against the state at impact time (BR-092, BR-095), so the flow checks targets at impact, not at the press.
- Melee affects every enemy within Attack Range in front of the player within the Attack Arc, once each (BR-115, BR-098); enemies beyond reach or behind the player are spared (BR-116).
- F-002 routes the attack input to the equipped weapon; F-004 defines the melee case, as F-003 defines the firearm case.
- When the run reaches Defeat or Victory, no further swing starts. A swing still pending at that moment applies no Damage and produces no hit confirmation (BR-014, BR-023, BR-102); the swing's visible form ends with it.

## Open questions

- None.
