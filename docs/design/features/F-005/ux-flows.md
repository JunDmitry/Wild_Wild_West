# UX Flows

Feature F-005 — Player health and death.

### FL-009 — Health, damage, and death during the run

Primary flow. Complete.

```mermaid
flowchart TD
  A(["run begins — Playing, player at full health"]) --> B["S-001 — Arena gameplay view · health indicator shows current = maximum"]
  B --> C{"has an enemy attack landed?"}
  C -- "no" --> B
  C -- "yes" --> D{"is the player's health still above zero?"}
  D -- "yes" --> E["S-001 — health indicator shows the reduced current health"]
  E --> C
  D -- "no" --> F["S-001 — health indicator reads zero; the player is defeated in the same operation"]
  F --> G(["defeated player — no movement, weapon switch, or attack; death lasts until a new run begins"])
```

### FL-010 — Damage at or beyond the remaining health

Edge flow. Complete.

```mermaid
flowchart TD
  A(["an enemy attack lands with damage at or beyond the player's remaining health"]) --> B{"does the attack's damage exceed the remaining health?"}
  B -- "yes" --> C["S-001 — current health becomes zero; applied damage equals only the health that remained"]
  B -- "no" --> D["S-001 — current health becomes zero exactly"]
  C --> E{"is any further damage applied?"}
  D --> E
  E -- "yes" --> F["S-001 — health stays at zero and never becomes negative"]
  E -- "no" --> G["S-001 — health stays at zero; the player remains defeated"]
  F --> G
```

### FL-011 — An attack that does not land changes nothing

Edge flow. Complete.

```mermaid
flowchart TD
  A(["an enemy attack resolves"]) --> B{"is the player within the attack's range at impact?"}
  B -- "no" --> C["S-001 — health unchanged; the indicator does not change"]
  B -- "yes" --> D["S-001 — health decreases by the attack's damage; the indicator updates at once"]
```

### FL-012 — A new run restores full health

Edge flow. Complete.

```mermaid
flowchart TD
  A(["a new run begins — the restart is F-010's trigger"]) --> B["S-001 — health indicator at maximum; the player is alive"]
  B --> C{"was the previous player defeated?"}
  C -- "yes" --> D["S-001 — the defeated state is cleared; movement, weapon switch, and attack act again"]
  C -- "no" --> E["S-001 — the new run starts at full health"]
  D --> F(["run Playing at full health"])
  E --> F
```

## Open questions

- None for F-005. The inherited F-001 FL-003 question — whether the view may rotate after the player dies — was settled by F-001b on 2026-10-23: the view freezes once the run ends.
