# F-005 — Player health and death

- **Source spec:** `docs/product/requirements/F-005-player-health-and-death.md`
- **Status:** handed-off
- **Handoff:** `docs/architect/design-inbox/F-005b.md`

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
| Accessibility | [`accessibility.md`](accessibility.md) | 2026-10-24 |

## Id index

Every flow and screen this feature mints, so an id traces back to its
feature. Ids are global to the role and never reused.

- Flows: `FL-009`, `FL-010`, `FL-011`, `FL-012`
- Screens: `S-001` (reused from F-001, extended by F-002; F-005 mints no screen id)

## Open questions

- None. The presentation of health is a design matter (US-019); the
  restart trigger belongs to F-010. The inherited F-001 FL-003 question
  on view rotation after death remains provisional there.
