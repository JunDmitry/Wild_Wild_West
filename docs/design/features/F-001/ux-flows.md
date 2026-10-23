# UX Flows

Feature F-001 — Player movement and third-person camera.

### FL-001 — Arena view and movement

Primary flow. Complete.

```mermaid
flowchart TD
  A(["run starts — Playing"]) --> B["S-001 — Arena gameplay view"]
  B --> C{"movement key held?"}
  C -- "no" --> B
  C -- "yes" --> D{"step stays inside Arena Bounds?"}
  D -- "yes" --> E["S-001 — moving relative to the current view"]
  D -- "no" --> F["S-001 — held at Arena Bounds"]
  E --> G{"mouse moved?"}
  F --> G
  G -- "yes" --> H["S-001 — view rotating around the player"]
  G -- "no" --> B
  H --> B
  B --> I{"run reaches Defeat or Victory?"}
  I -- "no" --> B
  I -- "yes" --> J(["movement input no longer changes the player's position"])
```

### FL-002 — Arena Bounds containment

Edge flow. Complete.

```mermaid
flowchart TD
  A(["movement input while at Arena Bounds"]) --> B{"does the input point outside?"}
  B -- "no" --> C["S-001 — moving inside the arena"]
  B -- "yes" --> D{"does the input also have a component along the boundary?"}
  D -- "yes" --> E["S-001 — sliding along Arena Bounds, remaining inside"]
  D -- "no" --> F["S-001 — stopped; a diagonal into a corner stays inside"]
```

### FL-003 — Movement and view after the run ends

Edge flow. Complete. Settled by F-001b.

```mermaid
flowchart TD
  A(["run reaches Defeat or Victory"]) --> B["S-001 — Arena gameplay view · terminal"]
  B --> C{"movement key held?"}
  C -- "yes" --> D["S-001 — terminal; player position unchanged"]
  C -- "no" --> B
  D --> B
  B --> E{"mouse moved?"}
  E -- "no" --> B
  E -- "yes" --> F["S-001 — terminal; the view holds its last direction"]
  F --> B
  B --> G{"does a new run begin?"}
  G -- "no" --> B
  G -- "yes — F-010's automatic restart" --> H(["new run — movement and view act again"])
```

## Open questions

- None. F-001b settles the only question F-001 carried: once the run ends — by Defeat or by Victory — the view no longer rotates in response to input and holds its last direction (spec R7, A6).
