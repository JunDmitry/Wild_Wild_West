# UX Flows

Feature F-003 — Firearm attack.

### FL-024 — Firearm attack during a run

Primary flow. Complete.

```mermaid
flowchart TD
  A(["run Playing; player alive; firearm equipped"]) --> B["S-001 — aiming marker shows the shot's direction along the current View Direction"]
  B --> C{"left mouse button pressed?"}
  C -- "no" --> B
  C -- "yes" --> D{"player alive, firearm equipped, no pending attack, and Cooldown elapsed?"}
  D -- "no" --> E["S-001 — no shot starts; the marker stays"]
  E --> B
  D -- "yes" --> F["S-001 — a shot starts along the aim direction"]
  F --> G{"at impact, is an enemy the nearest one along that direction within Attack Range?"}
  G -- "no" --> H["S-001 — the shot reaches no enemy; no health changes; no hit confirmation"]
  G -- "yes" --> I["S-001 — that enemy's health decreases by the firearm's damage"]
  I --> J["S-001 — hit confirmation appears at the damaged enemy at the moment of impact"]
  J --> K{"is the enemy's health now zero?"}
  K -- "yes" --> L["S-001 — the enemy is defeated and removed (F-007 and F-009)"]
  K -- "no" --> M["S-001 — the enemy remains alive with reduced health"]
  L --> N{"run terminal?"}
  M --> N
  H --> N
  N -- "no" --> B
  N -- "yes — Defeat or Victory" --> O(["no further shot starts; the marker and confirmations stop"])
```

### FL-025 — A shot that misses

Edge flow. Complete.

```mermaid
flowchart TD
  A(["a shot resolves at impact"]) --> B{"is an enemy within Attack Range along the aim direction?"}
  B -- "yes" --> C["S-001 — the nearest such enemy takes damage; hit confirmation appears"]
  B -- "no" --> D["S-001 — no enemy takes damage; every health value is unchanged"]
  D --> E["S-001 — no hit confirmation appears"]
```

### FL-026 — Firearm attack refused

Edge flow. Complete.

```mermaid
flowchart TD
  A(["left mouse button pressed"]) --> B{"is the player alive?"}
  B -- "no" --> C["S-001 — no attack starts; the marker does not change"]
  B -- "yes" --> D{"is the firearm equipped?"}
  D -- "no — melee weapon equipped" --> E["S-001 — no firearm shot starts; the melee attack is F-004's"]
  D -- "yes" --> F["S-001 — the firearm attack is available (FL-024)"]
```

### FL-027 — Firearm attack during Cooldown

Edge flow. Complete.

```mermaid
flowchart TD
  A(["firearm equipped; a shot is pending or the Cooldown has not elapsed; left mouse button pressed"]) --> B{"has the Cooldown elapsed?"}
  B -- "no" --> C["S-001 — no shot starts; the pending shot continues to its impact"]
  C --> D{"Cooldown elapsed later?"}
  D -- "no" --> C
  D -- "yes" --> E["S-001 — a later press starts a shot"]
  B -- "yes" --> E
```

### FL-028 — Aiming marker availability

Edge flow. Complete.

```mermaid
flowchart TD
  A(["the run state or the equipped weapon changes"]) --> B{"firearm equipped, player alive, and run Playing?"}
  B -- "yes" --> C["S-001 — the aiming marker is shown along the current View Direction"]
  B -- "no" --> D["S-001 — the aiming marker is absent"]
  C --> E{"view direction changes?"}
  E -- "yes" --> F["S-001 — the marker follows the new View Direction"]
  E -- "no" --> C
  F --> C
```

## Notes

- The aiming marker is present while the firearm is equipped, the player is alive, and the run is Playing. A momentary Cooldown does not hide it: R8's "can be fired" is read as R7's availability — equipped and alive — not as the instant readiness of one shot. The marker shows the current View Direction, which is the Aim Direction.
- The hit confirmation is tied to Damage actually applied. A shot that reaches no enemy produces none (R9, A9).
- Attack Start applies no Damage and Impact resolves against the state at impact time (BR-092, BR-095), so the flow checks the target at impact, not at the press.
- F-002 routes the attack input to the equipped weapon; F-003 defines what the firearm does once routed. F-004 defines the melee case.

## Open questions

- None.
