# F-011 — Victory after three waves

- **Source spec:** `docs/product/requirements/F-011-victory.md`
- **Status:** handed-off
- **Handoff:** `docs/architect/design-inbox/F-011.md`

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

- Flows: `FL-019`, `FL-020`, `FL-021`, `FL-022`, `FL-023`
- Screens: `S-002` (minted by F-011); `S-001` (reused from F-001; extended by F-002, F-005, and F-006; F-011 adds its post-victory state)

## Open questions

- None. The spec and the analysis settle the trigger, the terminal
  behaviour, and the exclusion of menu actions, score, rewards,
  animation, music, and camera work. The inherited F-001 FL-003
  view-rotation-after-death question remains provisional there and is
  untouched here.
