# F-009 — Boss enemy each wave

- **Source spec:** `docs/product/requirements/F-009-boss-enemy.md`
- **Status:** designed
- **Handoff:** —

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

- Flows: `FL-044`, `FL-045`, `FL-046`, `FL-047`
- Screens: `S-001` (reused from F-001, extended by F-002, F-003, F-005, F-006, F-007, and F-008) and `S-002` (F-011's Victory window, reached unchanged); F-009 mints no screen id

## Open questions

- None. The boss indicator's form, the marker's form, and the boss's
  health, size, and attack values are this feature's design and tuning
  decisions; the entry order and the wave-completion rule are fixed by
  the spec.
