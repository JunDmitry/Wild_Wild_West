# UX Flows

Feature F-011 — Victory after three waves.

### FL-019 — Victory after the third wave's boss is defeated

Primary flow. Complete.

```mermaid
flowchart TD
  A(["the run is Playing; the third Wave is in Boss Combat"]) --> B["S-001 — the player attacks the third Wave's Boss"]
  B --> C{"is the Boss defeated by this attack?"}
  C -- "no" --> A
  C -- "yes" --> D{"does any enemy of the third Wave remain?"}
  D -- "yes" --> E["S-001 — the Wave stays open; no window appears"]
  E --> F{"is the last remaining enemy defeated?"}
  F -- "no" --> E
  F -- "yes" --> G["S-001 — the third Wave completes"]
  D -- "no" --> G
  G --> H["the run becomes Victory in the same operation"]
  H --> I["S-002 — the Victory window appears over the arena view"]
  I --> J["S-001 — no further enemy enters; the arena stops changing"]
  J --> K(["the run is terminal; the window stays until the run is left"])
```

### FL-020 — A boss of the first or second wave is defeated

Edge flow. Complete.

```mermaid
flowchart TD
  A(["the run is Playing in Wave 1 or Wave 2 Boss Combat"]) --> B["S-001 — the player defeats that Wave's Boss"]
  B --> C{"does any enemy of that Wave remain?"}
  C -- "yes" --> D["S-001 — the Wave stays open; no window appears"]
  D --> C
  C -- "no" --> E["S-001 — the Wave completes"]
  E --> F["the next Wave begins in the same operation"]
  F --> G(["no Victory window; the run stays Playing"])
```

### FL-021 — The player is defeated during the third wave

Edge flow. Complete.

```mermaid
flowchart TD
  A(["the run is Playing; the third Wave is current"]) --> B["S-001 — the player's health reaches zero"]
  B --> C["the run becomes Defeat in the same operation"]
  C --> D["S-001 — no Victory window appears"]
  D --> E(["the run is terminal; F-010's automatic restart begins"])
```

### FL-022 — The third wave's boss is defeated while another enemy remains

Edge flow. Complete.

```mermaid
flowchart TD
  A(["the third Wave's Boss is defeated"]) --> B{"does any enemy of the third Wave remain?"}
  B -- "yes" --> C["S-001 — the Wave stays open; no window appears"]
  C --> D{"is the last remaining enemy defeated?"}
  D -- "no" --> C
  D -- "yes" --> E["S-001 — the third Wave completes"]
  B -- "no" --> E
  E --> F["the run becomes Victory in the same operation"]
  F --> G["S-002 — the Victory window appears over the arena view"]
```

### FL-023 — The player acts after the Victory window appears

Edge flow. Complete.

```mermaid
flowchart TD
  A(["S-002 — the Victory window is shown"]) --> B["the player presses movement, view, switch, or attack input"]
  B --> C["S-001 and S-002 — nothing changes; the run is terminal"]
  C --> D["no announcement is made for the refused input"]
  D --> A
```

## Open questions

- None. The trigger, the terminal behaviour, and the exclusion of
  actions, score, and animation are settled by the spec and the
  analysis. The inherited F-001 FL-003 view-rotation-after-death
  question remains provisional there and is untouched here.
