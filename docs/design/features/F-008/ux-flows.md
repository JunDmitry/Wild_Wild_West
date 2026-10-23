# UX Flows

Feature F-008 — Wave progression.

### FL-040 — Wave progression through a run

Primary flow. Complete.

```mermaid
flowchart TD
  A(["the run begins; the wave counter is at the first wave"]) --> B["S-001 — the wave indicator shows Wave 1 of 3; the first wave's announcement is shown"]
  B --> C{"are all enemies of the wave, including its boss, defeated?"}
  C -- "no" --> D["S-001 — the wave continues; the indicator does not change"]
  D --> C
  C -- "yes" --> E{"is this the third and final wave?"}
  E -- "no" --> F["S-001 — the next wave begins; the counter advances by one and the new-wave announcement is shown at the same moment"]
  F --> C
  E -- "yes" --> G(["S-002 — Victory window"])
```

### FL-041 — The first wave at run start

Edge flow. Complete.

```mermaid
flowchart TD
  A(["the run starts"]) --> B["S-001 — the wave indicator shows Wave 1 of 3"]
  B --> C["S-001 — the first wave's announcement is shown"]
  C --> D(["the first wave is in progress; no enemy of a later wave enters the arena"])
```

### FL-042 — The final wave is cleared

Edge flow. Complete.

```mermaid
flowchart TD
  A(["the last enemy of the third wave, including its boss, is defeated"]) --> B["S-001 — the wave indicator still shows Wave 3 of 3; it does not advance"]
  B --> C["S-002 — Victory window"]
  C --> D(["no fourth wave begins"])
```

### FL-043 — The run ends before the last wave

Edge flow. Complete.

```mermaid
flowchart TD
  A(["the player is defeated during a wave"]) --> B["S-001 — the wave indicator keeps showing the wave in progress; it does not advance"]
  B --> C{"does a new run begin?"}
  C -- "yes" --> D["S-001 — the counter returns to Wave 1 of 3 (FL-041)"]
  C -- "no" --> E(["the indicator stays frozen on the wave reached"])
```

## Notes

- The wave indicator is a persistent display on `S-001`: the current wave and the total, always three. It never shows a number the wave in progress does not have (A5).
- The announcement and the counter increase happen at the same moment (A6). The announcement is tied to the wave beginning, not to the previous wave being cleared.
- The first wave is announced at run start, so every wave begins the same way.
- The announcement is transient; its duration is a tuning value. The indicator is not transient.
- The final wave is the third; clearing it leads to the Victory window and no fourth wave (R6, A4).
- A defeat freezes the indicator on the wave reached; the defeat surface itself belongs to F-010, which reads the same number.
- F-008 adds no control and no new screen: `S-001` is extended, and `S-002` is reached unchanged.

## Open questions

- None.
