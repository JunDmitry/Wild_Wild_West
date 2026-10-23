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

### FL-003 — Movement after the run ends

Edge flow — `provisional`.

```mermaid
flowchart TD
  A(["run reaches Defeat or Victory"]) --> B["S-001 — Arena gameplay view · terminal"]
  B --> C{"movement key held?"}
  C -- "yes" --> D["S-001 — terminal; player position unchanged"]
  C -- "no" --> B
  D --> B
  B --> E{"mouse moved?"}
  E -- "no" --> B
  E -- "yes" --> F(["open question: may the view rotate after the run ends?"])
```

## Open questions

- FL-003 is provisional: the spec fixes movement only and does not state whether the view may still rotate after the player dies (US-005 note). Returned to `product-role`.
