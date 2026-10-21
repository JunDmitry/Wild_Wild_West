# US-022 — I never see an enemy appear

- **Feature:** F-006 — Enemy spawning outside the arena, unseen
- **Actor:** player
- **Priority:** P1 (from `docs/product/backlog.md`)
- **Rules:** `BR-073`, `BR-035`

## Story

As a player, I want to never see an enemy appear on screen, so that the
arena stays convincing and I cannot tell where the next threat will come
from.

## Examples

**E1 — the moment of appearing is out of view**
- Given a Spawn is due while the camera is aimed at the Arena,
- when the Enemy appears,
- then its appearance Position lies outside the player's current View.

**E2 — turning the camera reveals an enemy already in the arena**
- Given an Enemy appeared outside the View and has since entered,
- when the player turns the camera toward it,
- then the Enemy is already Active and moving — the player never observes
  it materialize.

**E3 — an enemy becomes visible only by entering**
- Given an Enemy appeared outside the View,
- when it crosses into the View,
- then it is visible because it came in from outside, not because it
  appeared where the player is looking.

## Notes

Spawn effects, telegraphs, and audio are out of scope. The View can turn
while the run is Playing, so the guarantee is about the moment of
appearing, not about a fixed camera angle.
