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
| F-003 | analyst-role | analyst-inbox | 2026-10-22T19:49:12Z | done | accepted; BR-112 stays with the adapter; Unity targeting resolver handed to `coder-role` 2026-10-22T19:53:33Z |
| c1 | coder-role | coder-inbox | 2026-10-22T19:58:22Z | done | answered 2026-10-22T19:59:51Z: ADR-0018, one `Game.Arena.PlayMode.Tests` assembly authorized (`docs/coder/architect-inbox/c1.md`) |
| F-004 | analyst-role | analyst-inbox | 2026-10-21T19:14:01Z | done | accepted; ADR-0019, Attack Arc in the Domain contract; melee resolver handed to `coder-role` 2026-10-22T20:12:34Z (`docs/coder/architect-inbox/F-004.md`) |
| c1b | coder-role | coder-inbox | 2026-10-22T20:06:05Z | done | answered 2026-10-22T20:20:20Z: ADR-0020, Presentation may reference Domain read-model value types; exact asmdef and boundary-test edits authorized (`docs/coder/architect-inbox/c1b.md`) |
