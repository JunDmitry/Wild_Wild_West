# UX Flows

Feature F-007 — Enemy pursuit and close-range attack.

### FL-035 — Pursuit, cue, and attack

Primary flow. Complete.

```mermaid
flowchart TD
  A(["run Playing; an Enemy is Active in the arena"]) --> B{"is the player within the Enemy's Attack Range?"}
  B -- "no" --> C["S-001 — the Enemy moves toward the player"]
  C --> B
  B -- "yes" --> D["S-001 — the Enemy stops closing and starts an attack; the Attack Cue is shown"]
  D --> E{"does the player remain within Attack Range until impact?"}
  E -- "yes" --> F["S-001 — the attack lands; the player's health decreases by its damage (F-005)"]
  E -- "no" --> G["S-001 — the attack misses; no health changes"]
  F --> H{"is the Enemy defeated or the run terminal?"}
  G --> H
  H -- "no" --> B
  H -- "yes" --> I(["the Enemy stops pursuing and attacking"])
```

### FL-036 — The player leaves range during the wind-up

Edge flow. Complete.

```mermaid
flowchart TD
  A(["an Enemy has started an attack; the Attack Cue is visible"]) --> B{"does the player move out of Attack Range before impact?"}
  B -- "yes" --> C["S-001 — the attack misses; the player's health is unchanged"]
  B -- "no" --> D["S-001 — the attack lands; health decreases by its damage"]
  C --> E{"does the player re-enter Attack Range?"}
  E -- "yes" --> F["S-001 — the Enemy attacks again when ready"]
  E -- "no" --> G["S-001 — the Enemy closes again"]
```

### FL-037 — The Enemy is defeated during the wind-up

Edge flow. Complete.

```mermaid
flowchart TD
  A(["an Enemy has started an attack; the Attack Cue is visible"]) --> B{"is the Enemy defeated before impact?"}
  B -- "yes" --> C["S-001 — the pending attack is cancelled; no health changes"]
  B -- "no" --> D["S-001 — the attack resolves at impact"]
  C --> E(["the Enemy is removed; its cue disappears with it"])
```

### FL-038 — The run ends during pursuit or wind-up

Edge flow. Complete.

```mermaid
flowchart TD
  A(["the run reaches Defeat or Victory"]) --> B["S-001 — no Enemy pursues or attacks further"]
  B --> C{"was an Enemy attack pending?"}
  C -- "yes" --> D["S-001 — the pending attack is cancelled; its cue disappears"]
  C -- "no" --> E["S-001 — the Enemy holds its position"]
  D --> F(["the run stays terminal until a new run begins"])
  E --> F
```

### FL-039 — An Enemy in range attacks without closing

Edge flow. Complete.

```mermaid
flowchart TD
  A(["an Enemy is Active"]) --> B{"is the player within its Attack Range?"}
  B -- "yes" --> C["S-001 — the Enemy does not close further"]
  C --> D{"is the Enemy ready and without a pending attack?"}
  D -- "yes" --> E["S-001 — the Enemy starts an attack; the Attack Cue is shown"]
  D -- "no" --> F["S-001 — the Enemy waits in range; no new cue"]
  B -- "no" --> G["S-001 — the Enemy pursues (FL-035)"]
```

## Notes

- Pursuit is in-world movement: the Enemy is seen closing on the player. It is not a HUD indicator and adds no control.
- The Attack Cue is a visible signal at the attacking Enemy. It is shown when the attack starts and remains visible until the attack resolves or is cancelled, so it always precedes the damage (R6, A6; BR-120).
- The cue's duration is a tuning value; its ordering before damage is fixed. The design does not fix a duration.
- The attack's impact checks range again (BR-100), so leaving range during the wind-up produces a miss, not damage.
- A defeated Enemy's pending attack is cancelled (BR-102), and the cue disappears with the Enemy.
- F-005 owns the health indicator and the damage display; F-007 adds the pursuit and the cue, not a new health surface.

## Open questions

- None.
