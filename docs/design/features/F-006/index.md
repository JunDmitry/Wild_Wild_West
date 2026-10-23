# F-006 — Enemy spawning outside the arena, unseen

- **Source spec:** `docs/product/requirements/F-006-enemy-spawning.md`
- **Status:** handed-off
- **Handoff:** `docs/architect/design-inbox/F-006.md`

## Artifacts

The six artifacts this feature owns. Update the date when an artifact
changes.

| Artifact | Link | Last updated |
|----------|------|--------------|
| UX flows | [`ux-flows.md`](ux-flows.md) | 2026-10-23 |
| Screens | [`screens.md`](screens.md) | 2026-10-23 |
| Wireframes | [`wireframes.md`](wireframes.md) | 2026-10-23 |
| Interaction | [`interaction.md`](interaction.md) | 2026-10-23 |
| States | [`states.md`](states.md) | 2026-10-23 |
| Accessibility | [`accessibility.md`](accessibility.md) | 2026-10-23 |

## Id index

Every flow and screen this feature mints, so an id traces back to its
feature. Ids are global to the role and never reused.

- Flows: `FL-013`, `FL-014`, `FL-015`, `FL-016`, `FL-017`, `FL-018`
- Screens: `S-001` (reused from F-001; extended by F-002 and F-005; F-006 mints no screen id)

## Open questions

- None. The analysis covers F-006 fully. Enemy count, spawn positions,
  and pacing are tuning values; spawn effects, telegraphs, audio, wave
  composition, and difficulty scaling are out of scope in the spec.
