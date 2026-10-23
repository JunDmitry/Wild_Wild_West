# UX Flows

Feature F-009 — Boss enemy each wave.

### FL-044 — Boss entry and the end of a wave

Primary flow. Complete.

```mermaid
flowchart TD
  A(["a wave is in progress; regular enemies are Active"]) --> B{"are all regular enemies of the wave defeated?"}
  B -- "no" --> C["S-001 — the wave continues; no boss appears"]
  C --> B
  B -- "yes" --> D["S-001 — a boss enters from outside the arena, marked distinctly from a regular enemy's entry"]
  D --> E["S-001 — the boss health indicator appears, showing the boss's remaining health"]
  E --> F{"does the boss take damage?"}
  F -- "yes" --> G["S-001 — the indicator decreases by the damage"]
  G --> H{"is the boss defeated?"}
  F -- "no" --> H
  H -- "no" --> I["S-001 — the boss keeps pursuing and attacking like any enemy (F-007)"]
  I --> F
  H -- "yes" --> J["S-001 — the indicator disappears; the wave is complete"]
  J --> K{"is this the third wave?"}
  K -- "no" --> L["S-001 — the next wave begins; the wave indicator advances and the announcement is shown (F-008)"]
  K -- "yes" --> M(["S-002 — Victory window"])
```

### FL-045 — A regular enemy is still alive

Edge flow. Complete.

```mermaid
flowchart TD
  A(["the wave is in progress and no boss has entered"]) --> B{"are all regular enemies of the wave defeated?"}
  B -- "no" --> C["S-001 — no boss enters; the wave continues"]
  C --> B
  B -- "yes" --> D["S-001 — the boss enters and its health indicator appears (FL-044)"]
```

### FL-046 — The boss is defeated and the wave completes

Edge flow. Complete.

```mermaid
flowchart TD
  A(["the boss's remaining health reaches zero"]) --> B["S-001 — the boss is removed; the boss health indicator disappears"]
  B --> C{"is this the third wave?"}
  C -- "no" --> D["S-001 — the next wave begins; the wave indicator advances and the announcement is shown (F-008)"]
  C -- "yes" --> E(["S-002 — Victory window"])
```

### FL-047 — The run ends with the boss alive

Edge flow. Complete.

```mermaid
flowchart TD
  A(["the player is defeated while the boss is alive"]) --> B["S-001 — the boss health indicator keeps the boss's last remaining health"]
  B --> C(["the run is terminal; the defeat surface belongs to F-010"])
```

## Notes

- The boss enters only after every regular enemy of the wave is defeated (R1, A1, A2). It enters from outside the arena, as regular enemies do (F-006), but its entry is marked distinctly (R6, A6).
- The boss health indicator appears with the boss and is shown only while it is alive (R7, A7). It is a HUD region labelled as the boss's, so it is never read as the player's health (F-005).
- The boss pursues and attacks like any other enemy (R3), so F-007's Attack Cue applies to it unchanged; F-009 adds no boss-specific attack surface.
- The wave is complete only when the boss is defeated (R4, A4); the next wave then begins (F-008), and the third boss's defeat leads to the Victory window (A5).
- The indicator disappears when the boss is defeated and does not linger into the next wave.
- The boss's health, size, and attack values are tuning values; the design fixes only what is shown.

## Open questions

- None.
