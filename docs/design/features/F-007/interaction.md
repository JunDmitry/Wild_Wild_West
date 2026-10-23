# Interaction

Feature F-007 — Enemy pursuit and close-range attack.

| control | behaviour | keyboard path | focus order |
|---------|-----------|---------------|-------------|
| Movement input | unchanged from F-001; moving can take the player out of an enemy's Attack Range during a wind-up, which makes that attack miss; moving does not change the enemy's reach or attack rate | W, A, S, D — unchanged from F-001 | 1 |
| View control | unchanged from F-001; turning the view changes what the player sees but not whether an enemy can reach them | mouse — unchanged from F-001 | 2 |
| Weapon-switch input | unchanged from F-002; switching does not stop an enemy's pursuit or cancel its attack | Q — unchanged from F-002 | 3 |
| Attack input | unchanged from F-002, F-003, and F-004; damaging or defeating an attacking enemy can cancel that enemy's pending attack (FL-037) | left mouse button — unchanged from F-002 | 4 |
| Enemy pursuit | every living enemy moves toward the player while the run is Playing and stops closing once within Attack Range; it resumes if the player moves away; never interactive | none — system behaviour | not in the focus order |
| Enemy Attack Cue | appears at the attacking enemy when its attack starts and stays visible until the attack resolves or is cancelled; always before the player's health changes; never interactive | none — display only | not in the focus order |

## Notes

- F-007 adds no interactive control and no new keyboard path. Pursuit and the cue are system behaviour observed through the view.
- The player's counter-play is movement and attack: leaving range during the wind-up avoids the damage (FL-036), and defeating the enemy cancels its pending attack (FL-037).
- The cue is a display, not a control: it cannot be focused, activated, or dismissed.
- A cue that ends without damage is silent on screen; the player sees the attack fail.
- Focus order continues the shared S-001 order from F-001 and F-002; the view has no tab stops and all four inputs act directly.
