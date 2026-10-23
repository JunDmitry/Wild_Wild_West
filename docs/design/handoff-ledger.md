# Design Handoff Ledger

The status the designer assigns to each message it has received. The
designer is the only writer. Status vocabulary follows
`role-base/references/handoffs.md`.

A message present in an inbox with no row here is `new` — present, not
yet accounted for.

| id | from | inbox | received | status | note |
|----|------|-------|----------|--------|------|
| F-001 | product-role | product-inbox | 2026-10-21T18:06:54Z | done | handoff `docs/architect/design-inbox/F-001.md`; FL-003 provisional on view rotation after death |
| F-002 | product-role | product-inbox | 2026-10-21T18:12:28Z | done | handoff `docs/architect/design-inbox/F-002.md`; S-001 extended with the equipped-weapon indicator; no provisional flows |
| F-005 | product-role | product-inbox | 2026-10-21T18:09:36Z | done | handoff `docs/architect/design-inbox/F-005.md`; S-001 extended with the health indicator; no provisional F-005 flows; inherited F-001 FL-003 view-rotation question remains provisional |
