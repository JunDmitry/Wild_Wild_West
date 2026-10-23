# Interaction

Feature F-009 — Boss enemy each wave.

| control | behaviour | keyboard path | focus order |
|---------|-----------|---------------|-------------|
| Movement input | unchanged from F-001; moving does not change the boss marker or its health indicator | W, A, S, D — unchanged from F-001 | 1 |
| View control | unchanged from F-001; turning the view does not change the boss health indicator | mouse — unchanged from F-001 | 2 |
| Weapon-switch input | unchanged from F-002; switching does not change the boss health indicator | Q — unchanged from F-002 | 3 |
| Attack input | unchanged from F-002, F-003, and F-004; damage to the boss lowers its remaining health and defeating it completes the wave | left mouse button — unchanged from F-002 | 4 |
| Boss entry marker | shown at the boss when it enters, so its entry is distinct from a regular enemy's; disappears once the entry is over; never interactive | none — display only | not in the focus order |
| Boss health indicator | shown while the boss is alive, decreasing as the boss takes damage; disappears when the boss is defeated; never interactive | none — display only | not in the focus order |

## Notes

- F-009 adds no interactive control and no new keyboard path. The marker and the indicator are displays observed through the view.
- The player fights the boss with the same inputs as any enemy; nothing about the boss is selectable, targetable, or focusable.
- The boss health indicator cannot be hidden, pinned, or dismissed by input; it is present exactly while the boss is alive.
- Focus order continues the shared S-001 order from F-001 and F-002; the view has no tab stops and all four inputs act directly.
