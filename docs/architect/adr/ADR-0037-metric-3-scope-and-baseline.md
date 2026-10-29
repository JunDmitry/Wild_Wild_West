# ADR-0037: Scope metric 3 to game-code allocation and bound the engine residual

- **Status:** Accepted
- **Date:** 2026-10-29
- **Authors:** architect-role
- **Reviewers:** user (Option A approved on 2026-10-29)
- **Related ADRs:** [ADR-0033](ADR-0033-performance-thresholds-for-cut-over.md), [ADR-0034](ADR-0034-automated-performance-measurement.md), [ADR-0035](ADR-0035-reusable-frame-buffers.md), [ADR-0036](ADR-0036-class-based-domain-events-for-pooling.md)

## Context

ADR-0033 metric 3 requires **0 B of managed allocation per frame after a 300-frame warm-up**, measured globally by one `GC.Alloc` recorder summed over the capture window. ADR-0035 and ADR-0036 made the Arena tick path allocate nothing that the code owns: the core fixture measures 0 B over 300 steady ticks and a ten-event frame, and the measured player allocation fell from 6 087 / 6 277 B per frame (runs 3–4) to 525 / 587 (runs 5–6) and then to **241.9 / 235.3 B per frame** at `2bd8fed`.

The residual is not reachable as a fixable call site. Attribution in the measurement player (`t9-gc-residual-b`) shows:

- disabling the tick (`-perfnotick`) reduces the run to **0.1 B/frame** — the allocation is entirely tick-triggered;
- skipping the player movement stage reduces it to **31.1 B/frame**, and skipping enemy movement to 158.1 — it tracks **motion of the rendered entities**;
- reducing the wave to one enemy leaves 212.4 B/frame, so it is not a per-enemy managed cost;
- the allocation call-stack capture bottoms out at `PlayerLoop/VContainerUpdate/UpdateFunction.Invoke()/GC.Alloc`, with **no game-code frame below the invoke**, and contains no such sample when the tick is disabled.

ADR-0034's 2026-10-28 amendment permits revising metric 3 only on a call-stack capture showing an allocation with no game-code call site. That capture now exists. A global 0 B target is therefore not a description of this game's code quality; it is a demand that Unity's own motion path stop allocating, which the project cannot deliver while the scene moves. Keeping the threshold unchanged would block T-9 for a cost the game does not own, while removing the allocation gate entirely would stop detecting real game regressions.

## Decision

We will replace ADR-0033 metric 3 with a **two-part rule**, and both parts must hold:

- **3a — Game-code allocation: 0 B/frame.** An allocation call-stack capture taken at the T-9 build must contain a **positive control** (a deliberate, known allocation made from a game-code method resolves to a game frame in the same capture) and must contain **no other game-code frame** as the allocating site. The A/B ladder corroborates: with the tick disabled the run stays at ≤ 1 B/frame.
- **3b — Total global allocation: ≤ 250 B/frame.** The existing `ProfilerRecorder` `GC.Alloc` sum, divided by captured frames, must not exceed the recorded baseline. The baseline is the two clean `2bd8fed` runs — **241.9 / 235.3 B/frame**; the threshold is the higher run rounded up to the next 10 B, and that rounding is the only headroom.

Concrete consequences:

- **Modules affected:** `Game.Performance` only — the probe's pass rule and the result schema. No production assembly, scene, prefab, or package changes.
- **New dependencies:** none.
- **Result schema:** `gcAlloc` gains `bytesPerFrame` and `baselineBytesPerFrame`; `pass.gcAlloc` reports 3b's comparison. 3a's status is recorded with the capture alongside the result, because a counter cannot verify a call stack.
- **Attribution obligation:** 3a is verified by one recorded capture at T-9 entry, not per run. 3b is checked automatically on every run.
- **Out of scope:** changing metrics 1, 2, 4, or 5; the legacy comparison; CI automation; memory footprint beyond per-frame allocations.

## Evidence

| Build | Variant | B/frame |
|---|---|---|
| `8251a54` | baseline | 243.7 / 246.0 |
| `8251a54` | `-perfnotick` | 0.1 |
| `8251a54` | `-perfskiptickstage=PlayerMovement` | 31.1 |
| `8251a54` | `-perfskiptickstage=EnemyMovement` | 158.1 |
| `8251a54` | `-perfwaveenemies=1` | 212.4 |
| `2bd8fed` | acceptance runs | **241.9 / 235.3** |

Capture at 10 enemies, 300-frame steady window: `UpdateFunction.Invoke()/GC.Alloc` — 17 100 B over 300/300 frames, no game-code frame below the invoke; the same capture with `-perfnotick` contains no such line. The event-scoped lines (TextMeshPro text reparse on an HUD change, view instantiation on an event frame, `EventSystem.Update`) are single-frame and below the baseline.

**Caveat, and why 3a demands a positive control.** A missing managed frame is not proof of a missing game call: player symbols can be stripped or incomplete, and an inlined game method can be collapsed out of a stack. The capture is therefore load-bearing only together with a positive control that shows a game-code allocation resolving as a game frame in the same build. The call-stack evidence is accepted here as sufficient to revise the threshold because the A/B ladder independently shows the allocation is tick- and motion-triggered, not that it is absent from game code.

## Options considered

### Option A — two-part rule: game-code 0 B plus a global baseline (chosen)
- Pros: keeps a real gate on what the code owns; the global threshold is a measured number, not an invented budget; catches regressions automatically; honest about the engine's motion cost.
- Cons: two parts, one of them a one-off capture rather than an automated check; the baseline must be re-derived if the measurement profile or a major path changes; accepts the engine residual up to 250 B/frame.
- Why chosen: it preserves the gate's meaning without demanding something the project cannot deliver.

### Option B — keep global 0 B and run one more attribution round
- Pros: the strictest gate; no ADR change; may find an avoidable site (camera follow, physics simulation, rendering, transform strategy).
- Cons: another full build/capture cycle; the allocation collapses only when the player stops moving, so the experiments may end where they started; blocks T-9 on an unowned cost.
- Why not chosen: the evidence already shows the cost is intrinsic to moving entities, and the gate's purpose is the game's own allocation.

### Option C — narrow metric 3 to game scope; report the global number informationally
- Pros: simplest; matches what can be attributed; no new threshold.
- Cons: waives automated protection against global allocation growth; a 3× regression in a game path that stayed under a manual capture would go unnoticed.
- Why not chosen: it removes the regression gate rather than re-anchoring it.

## Consequences

### Positive

- T-9 has a metric 3 that can pass, and it still fails on any material game-side allocation increase.
- The threshold rests on two recorded runs, and the derivation is reproducible.
- Four of the five ADR-0033 metrics are unchanged.

### Negative / trade-offs

- An engine residual of up to 250 B/frame is accepted as normal, and a slow drift under the threshold is invisible.
- 3a needs a human-verified capture at T-9 entry; a stripped-symbol capture must be rejected, which is a judgement call.
- The baseline is tied to the ADR-0034 measurement profile; a profile change requires a re-baseline.

### Neutral

- The metric's wording changes; its placement in the cut-over gate does not.
- `Builds/Performance/` stays uncommitted, so the capture is recorded as a table, not a file in the repository.

## Compliance with paradigm

- No deviation. This ADR adds no production type, no dependency, and no layering change; it revises a measurement threshold and the development-only harness that checks it.
- ADR-0007 is preserved: the metric is observed at the Unity boundary and never inside Domain or Application.
- The `Game.Performance` exception recorded under ADR-0034 is unchanged.

## Migration plan

1. Record this ADR; mark ADR-0033 metric 3 superseded and append a 2026-10-29 amendment to ADR-0034.
2. Extend the `Game.Performance` probe: compute `gcAlloc.bytesPerFrame`, compare it with `gcAlloc.baselineBytesPerFrame` (default 250, overridable by `-perfbaseline`), and report 3b in `pass.gcAlloc`. The capture protocol is unchanged.
3. Take the positive-control allocation capture and record it with the T-9 evidence.
4. Run two clean sessions at the implementation commit and append the rows to `docs/architect/migration/performance-measurement.md`.
5. T-9 passes metric 3 only when the capture is recorded and both clean runs are ≤ 250 B/frame.

Each step leaves the build green.

## References

- `docs/architect/coder-inbox/t9-gc-residual-b.md` — attribution report and capture
- `docs/architect/migration/performance-measurement.md` § Metrics and pass criteria, § Recording results
- `docs/architect/adr/ADR-0033-performance-thresholds-for-cut-over.md`
- `docs/architect/adr/ADR-0034-automated-performance-measurement.md`
- `Assets/_Project/Performance/` — measurement host and probe
