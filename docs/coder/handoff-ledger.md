# Coder Handoff Ledger

The status the coder assigns to each message it receives from
`architect-role` and `qa-role`. The coder is the only writer. Status
vocabulary follows `role-base/references/handoffs.md`.

A message present in an inbox with no row here is `new` — present,
not yet accounted for.

| id | from | inbox | received | status | note |
|----|------|-------|----------|--------|------|
| F-001 | architect-role | architect-inbox | 2026-10-22T19:49:27Z | done | coder task c1; Unity input adapter, movement resolver, presenter, and camera implemented — report `docs/architect/coder-inbox/F-001.md` |
| c1 | architect-role | architect-inbox | 2026-10-22T20:02:02Z | done | reply to coder request c1 — ADR-0018 authorizes `Game.Arena.PlayMode.Tests`; coder task c1 resumed and now complete |
| c1b | architect-role | architect-inbox | 2026-10-22T21:36:40Z | done | reply to coder request c1b — ADR-0020 allows `Game.Arena.Presentation` to reference `Game.Arena.Domain`; coder task c1 resumed and now complete |
| F-003 | architect-role | architect-inbox | 2026-10-22T20:14:43Z | done | coder task F-003; Unity targeting resolver and PlayMode tests (session coder-2026-10-22-201443-c908) — report `docs/architect/coder-inbox/F-003.md` |
| F-004 | architect-role | architect-inbox | 2026-10-22T20:12:34Z | done | coder task F-004; melee targeting resolver and PlayMode tests (session coder-2026-10-22-205044-5efbee) — report `docs/architect/coder-inbox/F-004.md` |
| F-003b | architect-role | architect-inbox | 2026-10-23T22:28:21Z | done | implementation-request; carried as the prerequisite of coder task `docs/coder/tasks/F-004b.md` and landed in the same change — damage notification, firearm aim marker, hit confirmation; report `docs/architect/coder-inbox/F-003b.md` |
| F-004b | architect-role | architect-inbox | 2026-10-23T22:43:47Z | done | implementation-request; coder task `docs/coder/tasks/F-004b.md` — melee swing, per-enemy confirmation, once-per-impact announcement (session coder-2026-10-23-234036-f004b); report `docs/architect/coder-inbox/F-004b.md` |
| F-001b | architect-role | architect-inbox | 2026-10-23T10:12:18Z | done | implementation-request; coder task `docs/coder/tasks/F-001b.md` — PlayMode bounds-sliding coverage (session coder-2026-10-24-005758-0f3137); report `docs/architect/coder-inbox/F-001b.md` |
