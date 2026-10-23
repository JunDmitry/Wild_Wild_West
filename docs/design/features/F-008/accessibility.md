# Accessibility

Feature F-008 — Wave progression.

| screen | what it needs | label | role | keyboard path | assistive-technology behaviour |
|--------|---------------|-------|------|---------------|--------------------------------|
| S-001 — Arena gameplay view | a labelled wave region that reads the current wave and the total; the announcement announced once when a wave begins; both distinguishable without relying on colour alone; the wave change is not signalled by the announcement alone | "Arena gameplay view" (unchanged from F-001); the wave region is labelled "Wave"; the announcement is labelled "New wave" | application (game view); the wave region is a status region and the announcement is an alert region | W, A, S, D move, mouse rotates the view, Q switches the weapon, left mouse button attacks — unchanged from F-001 and F-002; no tab traversal inside the view | the wave region is readable at any time and reads "Wave 2 of 3"; when a wave begins the announcement is announced once ("Wave 2 of 3 begins") at the same moment the region updates; it is not re-announced while it stays visible; the region is not announced per frame; the terminal state freezes the region on the wave reached |

## Focus order

- S-001 — Arena gameplay view: focus enters the view; neither the wave region nor the announcement is focusable and neither adds a tab stop; Tab order remains F-001's and F-002's — there are none inside the view; movement keys, mouse, Q, and the left mouse button act directly. The announcement does not take focus or trap it.
- S-002 — Victory window: unchanged from F-011; F-008 adds nothing.
