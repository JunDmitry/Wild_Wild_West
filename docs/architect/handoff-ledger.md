# Architect Handoff Ledger

The status the architect assigns to each message it has received. The
architect is the only writer. Status vocabulary follows
`role-base/references/handoffs.md`.

A message present in an inbox with no row here is `new` — present, not
yet accounted for.

| id | from | inbox | received | status | note |
|----|------|-------|----------|--------|------|
| F-001 | analyst-role | analyst-inbox | 2026-10-22T18:55:36Z | done | accepted; ADR-0017; implementation part handed to `coder-role` 2026-10-22T19:34:41Z (`docs/coder/architect-inbox/F-001.md`) |
| F-002 | analyst-role | analyst-inbox | 2026-10-22T19:44:04Z | done | already satisfied by existing architecture; no change, no handoff; task `docs/architect/tasks/F-002.md` |
| F-003 | analyst-role | analyst-inbox | 2026-10-22T19:49:12Z | done | accepted; BR-112 stays with the adapter; Unity targeting resolver handed to `coder-role` 2026-10-22T19:53:33Z; implementation report verified and accepted 2026-10-22T21:20:00Z |
| c1 | coder-role | coder-inbox | 2026-10-22T19:58:22Z | done | answered 2026-10-22T19:59:51Z: ADR-0018, one `Game.Arena.PlayMode.Tests` assembly authorized (`docs/coder/architect-inbox/c1.md`) |
| F-004 | analyst-role | analyst-inbox | 2026-10-21T19:14:01Z | done | accepted; ADR-0019, Attack Arc in the Domain contract; melee resolver handed to `coder-role` 2026-10-22T20:12:34Z (`docs/coder/architect-inbox/F-004.md`); implementation report verified and accepted 2026-10-22T21:28:17Z |
| c1b | coder-role | coder-inbox | 2026-10-22T20:06:05Z | done | answered 2026-10-22T20:20:20Z: ADR-0020, Presentation may reference Domain read-model value types; exact asmdef and boundary-test edits authorized (`docs/coder/architect-inbox/c1b.md`) |
| F-005 | analyst-role | analyst-inbox | 2026-10-21T19:23:11Z | done | already satisfied by existing architecture (Health, defeat guards, snapshot, restart); no change, no handoff; task `docs/architect/tasks/F-005.md` |
| F-006 | analyst-role | analyst-inbox | 2026-10-21T19:34:39Z | done | accepted; spawn rules already owned, BR-073 recorded as adapter-owned in the tick contract; Unity spawn-point provider handed to `coder-role` 2026-10-22T20:30:13Z (`docs/coder/architect-inbox/F-006.md`) |
| F-007 | analyst-role | analyst-inbox | 2026-10-21T19:40:14Z | done | accepted; pursuit and attack rules already Domain-owned, movement boundary recorded in the tick contract; Unity enemy movement resolver handed to `coder-role` 2026-10-22T20:33:09Z (`docs/coder/architect-inbox/F-007.md`) |
| F-008 | analyst-role | analyst-inbox | 2026-10-21T19:47:08Z | done | already satisfied by `Wave`/`WaveCatalog`/`ArenaRun` (incl. BR-064 successor numbering and victory); no change, no handoff; task `docs/architect/tasks/F-008.md` |
| F-009 | analyst-role | analyst-inbox | 2026-10-22T03:39:33Z | done | accepted; BR-063 enforced in `EnemyCatalog` with Domain tests; other boss rules already owned; no handoff; task `docs/architect/tasks/F-009.md` |
| F-010 | analyst-role | analyst-inbox | 2026-10-22T03:46:13Z | done | accepted; ADR-0021, automatic defeat restart owned by the Application tick; implemented in `933ec0e`; no handoff; task `docs/architect/tasks/F-010.md` |
| F-011 | analyst-role | analyst-inbox | 2026-10-22T03:54:07Z | done | accepted; ADR-0022, Application notifications for one-shot facts; victory published through a sink port; no coder handoff; task `docs/architect/tasks/F-011.md` |
