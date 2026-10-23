# Interaction

Feature F-008 — Wave progression.

| control | behaviour | keyboard path | focus order |
|---------|-----------|---------------|-------------|
| Movement input | unchanged from F-001; moving does not change the wave in progress or the counter | W, A, S, D — unchanged from F-001 | 1 |
| View control | unchanged from F-001; turning the view does not change the wave indicator | mouse — unchanged from F-001 | 2 |
| Weapon-switch input | unchanged from F-002; switching does not change the wave indicator | Q — unchanged from F-002 | 3 |
| Attack input | unchanged from F-002, F-003, and F-004; defeating a wave's last enemy, including its boss, begins the next wave | left mouse button — unchanged from F-002 | 4 |
| Wave indicator | persistent display of the current wave and the total of three; advances only when a new wave begins; never interactive | none — display only | not in the focus order |
| New-wave announcement | appears when a wave begins, at the same moment the indicator advances; transient and self-dismissing; cannot be dismissed, paused, or advanced by input; never interactive | none — display only | not in the focus order |

## Notes

- F-008 adds no interactive control and no new keyboard path. The wave indicator and the announcement are displays observed through the view.
- The announcement is not modal: it does not take focus, block input, or pause the arena. The player keeps moving, looking, switching, and attacking underneath it.
- There is no way to skip, replay, or recall an announcement; it fades on its own after its tuning duration.
- The wave changes as a consequence of defeating the wave's enemies, not of any input; no control advances it.
- Focus order continues the shared S-001 order from F-001 and F-002; the view has no tab stops and all four inputs act directly.
