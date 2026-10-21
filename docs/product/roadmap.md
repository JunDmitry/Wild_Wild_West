# Roadmap

Milestones in order. Each has an entry and an exit condition and names
the backlog features it contains. No dates — a milestone commits to
order and to a done-condition, not to a calendar.

### M1 — Combat foundations

Entry: the Arena Combat core builds and an arena scene is available to play in.
Exit:  in the arena, the player moves and looks around in third person, switches weapons with Q, and can damage an enemy with both the firearm and melee.

- F-001  Player movement and third-person camera
- F-002  Weapon switching
- F-003  Firearm attack
- F-004  Melee attack

### M2 — Survival loop

Entry: M1 has exited.
Exit:  enemies enter unseen from outside the arena, pursue and attack the player, waves advance only when cleared, and a run can end in defeat with an automatic restart that resets the wave counter.

- F-005  Player health and death
- F-006  Enemy spawning outside the arena, unseen
- F-007  Enemy pursuit and close-range attack
- F-008  Wave progression
- F-010  Defeat and scene restart

### M3 — Bosses and victory

Entry: M2 has exited.
Exit:  a boss closes each of the three waves, and defeating the third boss ends the run with the Victory window.

- F-009  Boss enemy each wave
- F-011  Victory after three waves

<!--
  Milestone names are free-form (v0.2, M1, "Playable core", ...).
  Every id named here must be a row in docs/product/backlog.md.
  A feature appears in exactly one milestone.
-->
