# Interaction

Feature F-011 — Victory after three waves.

| control | behaviour | keyboard path | focus order |
|---------|-----------|---------------|-------------|
| Movement input | unchanged from F-001 while the run is Playing; after Victory it changes nothing and the player stays where the winning attack left them | W, A, S, D — unchanged from F-001 | 1 |
| View control | unchanged from F-001 while the run is Playing; after Victory the view no longer rotates because S-002 holds the focus | mouse — unchanged from F-001 | 2 |
| Weapon-switch input | unchanged from F-002 while the run is Playing; after Victory it changes nothing and the equipped weapon stays as it was | Q — unchanged from F-002 | 3 |
| Attack input | unchanged from F-002, F-003, and F-004 while the run is Playing; after Victory the player can start no attack and an attack input changes nothing | left mouse button — unchanged from F-002 | 4 |
| Victory window (S-002) | no control: it appears when the third Wave completes, cannot be activated or dismissed, and holds the focus while it is shown | none — no key reaches or activates it | 5 |

## Notes

- F-011 adds no interactive control and no new keyboard path. The window is not a menu: it has no action, no dismissal, and no tab stop.
- Focus moves from S-001 into S-002 when the window appears and stays there for as long as the run is shown.
- The four gameplay inputs remain direct inputs while the run is Playing. After Victory they are inert: they change nothing and are not announced.
- A refused input after Victory is silent. No state is shown for it, and the window does not react.
- The inherited F-001 FL-003 view-rotation-after-death question is untouched. F-011 settles only the post-Victory case, where the window holds the focus and the view does not rotate.
- Focus order continues the shared S-001 order from F-001, F-002, and F-005; the window is the only element that ever receives focus besides the view.
