# UX Flows

Feature F-006 — Enemy spawning outside the arena, unseen.

### FL-013 — Enemies enter the arena from beyond its bounds, unseen

Primary flow. Complete.

```mermaid
flowchart TD
  A(["a Wave is active and the run is Playing"]) --> B{"is a Spawn due on the Wave's Spawn Pacing?"}
  B -- "no" --> A
  B -- "yes" --> C["S-001 — a position in the Spawn Band outside Arena Bounds and outside the current view is chosen"]
  C --> D["S-001 — the Enemy appears at that position, Spawned and unobserved"]
  D --> E["S-001 — the Enemy moves inward toward the arena"]
  E --> F{"has the Enemy crossed Arena Bounds?"}
  F -- "no" --> E
  F -- "yes" --> G["S-001 — the Enemy is Active inside the arena: visible, alive, and counted toward the Wave"]
  G --> H{"do more Enemies of this phase remain to spawn?"}
  H -- "yes" --> B
  H -- "no" --> I(["the phase's spawning is done; the Wave stays open until its Enemies are defeated"])
```

### FL-014 — A due spawn while the player looks toward the Spawn Band

Edge flow. Complete.

```mermaid
flowchart TD
  A(["a Spawn falls due"]) --> B["S-001 — the Spawn Band is considered around the arena, outside Arena Bounds"]
  B --> C{"is the candidate position outside the player's current view?"}
  C -- "no" --> D["S-001 — the position is not eligible; another position in the band is chosen"]
  D --> C
  C -- "yes" --> E["S-001 — the Enemy appears at the eligible position, outside the view"]
  E --> F["S-001 — the Enemy moves inward and becomes visible only after entering the arena"]
```

### FL-015 — The player turns the view during an entry

Edge flow. Complete.

```mermaid
flowchart TD
  A(["an Enemy is Spawned outside Arena Bounds and moving inward"]) --> B["S-001 — the player turns the view"]
  B --> C{"does the new view include the Enemy's position?"}
  C -- "yes" --> D["S-001 — the Enemy is seen moving inward; it is never seen appearing"]
  C -- "no" --> E["S-001 — the Enemy remains outside the view"]
  D --> F{"has the Enemy crossed Arena Bounds?"}
  E --> F
  F -- "no" --> B
  F -- "yes" --> G["S-001 — the Enemy is Active inside the arena and counted toward the Wave"]
```

### FL-016 — Regular and Boss spawns belong to their combat phases

Edge flow. Complete.

```mermaid
flowchart TD
  A(["a Wave begins in Regular Combat"]) --> B["S-001 — Regular Enemies enter on the Wave's Spawn Pacing"]
  B --> C{"are the Wave's Regular Enemies done spawning and defeated?"}
  C -- "no" --> B
  C -- "yes" --> D(["Boss Combat begins — the Boss's composition and toughness are F-009's"])
  D --> E["S-001 — the Boss enters from beyond Arena Bounds under the same unseen-entry rule"]
  E --> F["S-001 — the Boss is Active inside the arena and counted toward the Wave"]
```

### FL-017 — A later Wave's enemies wait their turn

Edge flow. Complete.

```mermaid
flowchart TD
  A(["Wave N is current"]) --> B{"is the due Enemy part of Wave N?"}
  B -- "yes" --> C["S-001 — the Enemy enters on Wave N's Spawn Pacing"]
  B -- "no" --> D["S-001 — the later Wave's Enemy is not due and does not enter"]
  D --> E{"has Wave N completed?"}
  E -- "no" --> A
  E -- "yes" --> F["S-001 — Wave N+1 becomes current and its Enemies enter on its own pacing"]
```

### FL-018 — Spawning stops when the run ends

Edge flow. Complete.

```mermaid
flowchart TD
  A(["the run reaches a terminal state"]) --> B{"is it Defeat or Victory?"}
  B -- "Defeat" --> C["S-001 — no further Spawn is processed; entered Enemies remain where the terminal state leaves them"]
  B -- "Victory" --> D["S-001 — no further Spawn is processed; entered Enemies remain where the terminal state leaves them"]
  C --> E{"does a new run begin?"}
  D --> E
  E -- "yes (the restart is F-010's trigger)" --> F["S-001 — the new run begins at Wave 1 with its own Spawn Pacing"]
  E -- "no" --> G(["no Enemy enters; the run stays terminal"])
```

## Open questions

- None for F-006. Enemy count, spawn positions, and pacing are tuning
  values; spawn effects, telegraphs, audio, wave composition, and
  difficulty scaling are out of scope in the spec.
