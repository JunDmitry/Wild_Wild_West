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
| F-006 | product-role | product-inbox | 2026-10-21T18:09:50Z | done | handoff `docs/architect/design-inbox/F-006.md`; S-001 extended with enemy entry; no new screen; no provisional F-006 flows |
| F-011 | product-role | product-inbox | 2026-10-21T18:11:06Z | done | handoff `docs/architect/design-inbox/F-011.md`; S-002 Victory window minted; S-001 post-victory state; no provisional F-011 flows; inherited F-001 FL-003 view-rotation question remains provisional |
| F-001b | product-role | product-inbox | 2026-10-23T18:25:09Z | done | handoff `docs/architect/design-inbox/F-001b.md` (supersedes F-001); FL-003 settled — the view freezes on Defeat and Victory; F-005/F-011 caveats corrected |
| F-003 | product-role | product-inbox | 2026-10-23T18:44:22Z | done | handoff `docs/architect/design-inbox/F-003.md`; S-001 extended with the aiming marker and hit confirmation; no new screen; no provisional flows |
| F-004 | product-role | product-inbox | 2026-10-23T18:44:22Z | done | handoff `docs/architect/design-inbox/F-004.md`; S-001 extended with the melee swing and hit confirmation; no new screen; no provisional flows |
| F-007 | product-role | product-inbox | 2026-10-23T18:44:22Z | done | handoff `docs/architect/design-inbox/F-007.md`; S-001 extended with pursuit and the Attack Cue; no new screen; no provisional flows |
| F-008 | product-role | product-inbox | 2026-10-23T18:44:22Z | done | handoff `docs/architect/design-inbox/F-008.md`; S-001 extended with the wave indicator and the new-wave announcement; S-002 reached unchanged; no new screen; no provisional flows |
| F-009 | product-role | product-inbox | 2026-10-23T18:44:22Z | done | handoff `docs/architect/design-inbox/F-009.md`; S-001 extended with the boss entry marker and the boss health indicator; S-002 reached unchanged; no new screen; no provisional flows |
| F-010 | product-role | product-inbox | 2026-10-23T18:44:22Z | done | handoff `docs/architect/design-inbox/F-010.md`; S-003 Defeat window minted, distinct from S-002; no provisional flows |
| F-004 | architect-role | architect-inbox | 2026-10-23T22:55:45Z | done | rework answered by `docs/architect/design-inbox/F-004b.md` (supersedes `architect/design-inbox/F-004.md`); swing now follows the live view direction until impact |
