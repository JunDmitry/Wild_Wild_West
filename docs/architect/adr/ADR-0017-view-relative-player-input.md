# ADR-0017: Convert view-relative player input in the Application

- **Status:** Accepted
- **Date:** 2026-10-22
- **Authors:** architect-role
- **Reviewers:** none
- **Related ADRs:** [ADR-0007](ADR-0007-unity-isolation-and-layer-dependencies.md), [ADR-0011](ADR-0011-domain-authority-over-movement-resolution.md)

## Context

Feature F-001 requires player movement to follow the current third-person
view: W, A, S, and D are interpreted relative to where the camera looks. The
analyst defines View Direction as the **horizontal** direction the camera
faces, and records that the Aim Direction of both weapons is that same
current View Direction (`docs/analyst/domain-model.md`).

Before this decision, `PlayerFrameInput` carried an already world-space
`MovementInput` and an `AimDirection`. That placed the camera-relative
conversion in the Unity input adapter: gameplay interpretation would live in
an adapter, depend on a scene `Transform`, and could only be tested in
PlayMode.

The Domain already owns the rules the converted value must satisfy:
`MovementInput` rejects non-planar input and normalizes magnitude to one,
`ArenaRun.RequestPlayerMovement` rejects movement outside Playing, and
`ArenaBounds.PermittedTravel` limits the requested distance.

## Decision

The Application input contract carries device-relative input, and the
Application performs the view-relative conversion.

`PlayerFrameInput` contains:

- `MovementAxes` — forward and right axes relative to the view;
- `ViewDirection` — the current horizontal, unit-length view direction;
- the attack-start and weapon-switch requests.

The Application translates the axes into a planar world-space
`MovementInput` using the View Direction as forward and its perpendicular as
right, clamps the resulting magnitude to one so diagonal input is not faster,
and uses the View Direction directly as the Aim Direction.

Consequences:

- modules affected: `Game.Arena.Application/Input` and the two player tick
  stages that consumed `Movement` and `AimDirection`;
- no new port and no new assembly; `IPlayerInputSource` keeps its shape;
- the Unity input adapter only reads devices and the camera's horizontal
  forward — it performs no gameplay math;
- Domain rules, contracts, and Arena Bounds behaviour are unchanged;
- explicitly out of scope: camera pitch, camera collision, and view rotation
  after defeat (presentation and design concerns).

## Rationale

- Input interpretation is a use-case concern, not an adapter concern; it
  belongs in the Application layer that already owns `PlayerFrameInput`.
- Pure geometry over value types is unit-testable without Unity, which keeps
  the standalone `dotnet test` suite authoritative for this rule.
- The analyst's "View Direction is horizontal" makes the conversion a planar
  rotation with no camera projection; the adapter needs only the camera's
  horizontal forward.
- The accepted ADR-0011 boundary stays intact: the Domain still decides how
  far and where the Player may move.

## Consequences

### Positive

- The camera-relative rule (BR-032) is covered by Application tests.
- Adapters become thin device readers and stay replaceable.
- The View Direction is supplied once per frame and is the single source for
  movement direction and aim.
- The public contract names the domain concepts the analyst defined.

### Negative

- `PlayerFrameInput` changes shape; every construction site and test is
  updated.
- The contract requires a horizontal view direction; producing it is the
  adapter's and presentation's responsibility.
- View-relative conversion now runs per frame in the Application instead of
  in the adapter (negligible cost).

### Neutral / Risks

- A degenerate view direction (no horizontal component) yields zero movement
  rather than an exception; the third-person camera must clamp pitch.
- Future vertical aiming would require a new ADR, because the contract is
  deliberately horizontal.

## Alternatives

1. **Scene-shared camera Transform** — the input adapter computes world-space
   movement from the camera directly.
   - Why not chosen: gameplay interpretation stays in an adapter and is hard
     to test without a scene.
2. **`IViewDirectionSource` port** — Presentation implements a port consumed
   by the input adapter.
   - Why not chosen: an extra public contract and per-frame read path for a
   single consumer, while the adapter still reads a view direction.

## Compliance with paradigm

- ADR-0007: Application still references no Unity type; the contract uses
  `MovementAxes` and `Direction3D` only.
- ADR-0011: the Domain remains the authority over accepted movement; the
  Application only interprets device input into the Domain's planar contract.
- `docs/architect/paradigm.md`: no deviation; no exception is added.

## Migration plan

1. Add `MovementAxes` and `ViewRelativeMovement` to `Application/Input`.
2. Change `PlayerFrameInput` to axes + horizontal view direction.
3. Update `PlayerMovementStage` (translate) and `PlayerAttackImpactStage`
   (use the view direction).
4. Update `application-tick-contract.md` § Input.
5. Update the affected Application tests and add translation tests.

Each step leaves the standalone build and tests green.

## References

- `docs/architect/analyst-inbox/F-001.md` — stories US-001…US-005, rules
  BR-030…BR-038
- `docs/analyst/domain-model.md` — View Direction, Aim Direction
- `docs/analyst/business-rules.md` — BR-032, BR-111
- `docs/architect/architecture/arena-combat/application-tick-contract.md`
- `Assets/_Project/Contexts/Arena/Application/Input/`
- `docs/coder/delivery-plan.md` — T-6.2 (Unity input adapter)
