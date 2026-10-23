# Interaction

Feature F-006 — Enemy spawning outside the arena, unseen.

| control | behaviour | keyboard path | focus order |
|---------|-----------|---------------|-------------|
| Movement input | unchanged from F-001; moving does not change when or where enemies enter; the player may close with or avoid an enemy already inside | W, A, S, D — unchanged from F-001 | 1 |
| View control | unchanged from F-001; turning the view can reveal an enemy that is already Spawned and moving inward, but never a spawn moment; a position inside the view is not eligible for a new spawn | mouse — unchanged from F-001 | 2 |
| Weapon-switch input | unchanged from F-002; switching does not affect entry | Q — unchanged from F-002 | 3 |
| Attack input | unchanged from F-002, F-003, and F-004; F-006 adds no attack behaviour and does not change when an attack can reach an enemy | left mouse button — unchanged from F-002 | 4 |

## Notes

- F-006 adds no interactive control, no new keyboard path, and no new tab stop. Enemy entry is a system behaviour observed through S-001.
- The player influences entry only through the view: a spawn position is chosen outside the current view, so turning toward the Spawn Band makes positions there ineligible for the next spawn.
- A Spawned enemy is not a control and cannot be focused, activated, or dismissed. It becomes observable only after it crosses Arena Bounds and is Active.
- A refused or ineligible spawn position changes nothing on screen and is not announced; another position is chosen.
- Focus order continues the shared S-001 order from F-001, F-002, and F-005; the view has no tab stops and all four inputs act directly.
