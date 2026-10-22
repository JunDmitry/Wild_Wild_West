# Architect Handoff Ledger

The status the architect assigns to each message it has received. The
architect is the only writer. Status vocabulary follows
`role-base/references/handoffs.md`.

A message present in an inbox with no row here is `new` — present, not
yet accounted for.

| id | from | inbox | received | status | note |
|----|------|-------|----------|--------|------|
| F-001 | analyst-role | analyst-inbox | 2026-10-22T18:55:36Z | done | accepted; ADR-0017; implementation part handed to `coder-role` 2026-10-22T19:34:41Z (`docs/coder/architect-inbox/F-001.md`) |
