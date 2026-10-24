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
| F-002 | architect-role | architect-inbox | 2026-10-23T10:37:25Z | done | implementation-request; coder task `docs/coder/tasks/F-002.md` — equipped-weapon indicator (session coder-2026-10-24-012919-3c35b0); report `docs/architect/coder-inbox/F-002.md` |
| F-002b | architect-role | architect-inbox | 2026-10-23T10:54:41Z | done | supersedes the binding in `F-002` requirements 2–3; carried by coder task `docs/coder/tasks/F-002.md` — HUD binding implemented |
| F-001c | architect-role | architect-inbox | 2026-10-23T22:21:47Z | done | implementation-request; coder task `docs/coder/tasks/F-001c.md` — camera hold while the run is terminal (session coder-2026-10-24-015649-f1c9a2); report `docs/architect/coder-inbox/F-001c.md` |
| F-002c | architect-role | architect-inbox | 2026-10-23T22:07:19Z | done | implementation-request; coder task `docs/coder/tasks/F-002c.md` — BR-119 per-weapon readiness regression test, no production change (session coder-2026-10-24-022949-7b81fb); report `docs/architect/coder-inbox/F-002c.md` |
| F-007c | architect-role | architect-inbox | 2026-10-23T22:54:08Z | done | implementation-request; coder task `docs/coder/tasks/F-007c.md` — enemy attack cue and outcome announcements (session coder-2026-10-24-024224-f007c); report `docs/architect/coder-inbox/F-007c.md` |
| F-005 | architect-role | architect-inbox | 2026-10-23T10:54:41Z | done | implementation-request; coder task `docs/coder/tasks/F-005.md` — health indicator on the arena gameplay view (session coder-2026-10-24-031426-883435); report `docs/architect/coder-inbox/F-005.md` |
| F-008b | architect-role | architect-inbox | 2026-10-23T23:02:09Z | done | implementation-request; coder task `docs/coder/tasks/F-008b.md` — wave indicator text and the new-wave announcement (session coder-2026-10-24-033000-f59a28); report `docs/architect/coder-inbox/F-008b.md`; supersedes F-010b requirement 8's second sentence |
| F-009b | architect-role | architect-inbox | 2026-10-23T23:12:26Z | done | implementation-request; coder task `docs/coder/tasks/F-009b.md` — boss health region and boss entry (session coder-2026-10-24-034811-64a2dc); report `docs/architect/coder-inbox/F-009b.md` |
| F-006 | architect-role | architect-inbox | 2026-10-24T04:11:38Z | done | implementation-request; coder task `docs/coder/tasks/F-006.md` — Unity spawn-point provider (session coder-2026-10-24-041138-f006); report `docs/architect/coder-inbox/F-006.md` |
| F-006b | architect-role | architect-inbox | 2026-10-23T11:03:14Z | done | implementation-request; coder task `docs/coder/tasks/F-006b.md` — enemy entry visibility and once-only announcement (session coder-2026-10-24-042637-1d07); report `docs/architect/coder-inbox/F-006b.md` |
| F-011 | architect-role | architect-inbox | 2026-10-23T11:08:40Z | done | implementation-request; coder task `docs/coder/tasks/F-011.md` — victory window (S-002), Presentation side of the notification sink (session coder-2026-10-24-044042-ccbdaa); report `docs/architect/coder-inbox/F-011.md` |
| F-010b | architect-role | architect-inbox | 2026-10-23T20:59:12Z | done | implementation-request; coder task `docs/coder/tasks/F-010b.md` — defeat window (S-003) over published defeat state (session coder-2026-10-24-051259-bb2be8); report `docs/architect/coder-inbox/F-010b.md`; Presentation only, no Domain or Application change |
| F-007 | architect-role | architect-inbox | 2026-10-22T20:33:09Z | done | implementation-request; coder task `docs/coder/tasks/F-007.md` — Unity enemy movement resolver (T-6.6) and PlayMode coverage (session coder-2026-10-24-052828-f60f95); report `docs/architect/coder-inbox/F-007.md` |
