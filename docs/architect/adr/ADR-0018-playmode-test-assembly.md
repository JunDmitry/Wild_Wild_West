# ADR-0018: PlayMode tests live in a separate Unity test assembly

- **Status:** Accepted
- **Date:** 2026-10-22
- **Authors:** architect-role
- **Reviewers:** none
- **Related ADRs:** [ADR-0007](ADR-0007-unity-isolation-and-layer-dependencies.md), [ADR-0015](ADR-0015-standalone-dotnet-build-for-arena-core.md)

## Context

F-001 (player movement and camera) and F-003 (firearm targeting resolver) are
Unity-side implementations whose acceptance requires PlayMode tests: the
input adapter reads `UnityEngine.Input`, the movement resolver applies a
position to a scene transform, and the presenter and camera controller are
`MonoBehaviour`s that must run in play mode.

Every test assembly in the repository is Editor-only:

- `Game.Arena.Domain.Tests`, `Game.Arena.Application.Tests`,
  `Game.Arena.Infrastructure.Tests`, `Game.Architecture.Tests`, and
  `Game.Legacy.Characterization.Tests` set
  `"includePlatforms": ["Editor"]` — Unity treats that as EditMode.
- `Game.Arena.Presentation` is a runtime assembly
  (`"includePlatforms": []`) and no test assembly references it.
- `AssemblyBoundaryTests` exempts assemblies whose names end in `.Tests`
  from the production dependency contract.

`coder-role` is not allowed to create or edit `.asmdef` files, so
`docs/architect/coder-inbox/c1.md` defers the work to the architect.

## Decision

Add **one** PlayMode test assembly:

`Assets/_Project/Contexts/Arena/Tests/GameArenaPlayMode/Game.Arena.PlayMode.Tests.asmdef`

```json
{
    "name": "Game.Arena.PlayMode.Tests",
    "rootNamespace": "Game.Arena.PlayMode.Tests",
    "references": [
        "UnityEngine.TestRunner",
        "UnityEditor.TestRunner",
        "Game.Arena.Domain",
        "Game.Arena.Application",
        "Game.Arena.Infrastructure",
        "Game.Arena.Presentation"
    ],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": true,
    "precompiledReferences": [
        "nunit.framework.dll"
    ],
    "autoReferenced": false,
    "defineConstraints": [
        "UNITY_INCLUDE_TESTS"
    ],
    "versionDefines": [],
    "noEngineReferences": false
}
```

Rules for its contents:

- It holds only tests that need the Unity runtime — `MonoBehaviour`
  lifecycle, physics colliders, cameras, and the frame loop.
- Pure logic tests stay in the Editor-only assemblies and in the standalone
  `dotnet test` suites.
- Existing Editor-only test assemblies are unchanged.

## Amendment 2026-10-25 — composition-root PlayMode test assembly

T-8.1's composition fixture cannot live in `Game.Arena.PlayMode.Tests`. C#
assembly references are not transitive, so the fixture cannot reach
`Game.CompositionRoot` or `VContainer` through the Arena layer references, and
widening the Arena assembly would put the composing layer inside a test
assembly that owns Arena behavior. A second PlayMode test assembly is added
instead:

`Assets/_Project/CompositionRoot/Tests/GameCompositionRootPlayMode/Game.CompositionRoot.PlayMode.Tests.asmdef`

```json
{
    "name": "Game.CompositionRoot.PlayMode.Tests",
    "rootNamespace": "Game.CompositionRoot.PlayMode.Tests",
    "references": [
        "UnityEngine.TestRunner",
        "UnityEditor.TestRunner",
        "Game.CompositionRoot",
        "Game.Arena.Domain",
        "Game.Arena.Application",
        "VContainer"
    ],
    "includePlatforms": [],
    "excludePlatforms": [],
    "allowUnsafeCode": false,
    "overrideReferences": true,
    "precompiledReferences": [
        "nunit.framework.dll"
    ],
    "autoReferenced": false,
    "defineConstraints": [
        "UNITY_INCLUDE_TESTS"
    ],
    "versionDefines": [],
    "noEngineReferences": false
}
```

Rules:

- It holds only composition-root PlayMode tests: scope building, registration
  completeness, missing-seam failures, and singleton identity.
- It is test-only — `UNITY_INCLUDE_TESTS`, `autoReferenced: false`, and a name
  ending in `.Tests` — so `AssemblyBoundaryTests` exempts it from the production
  dependency contract, exactly as for the Arena test assembly.
- `Game.Arena.PlayMode.Tests` is unchanged; Arena behavior tests stay there.
- It references the Arena Domain and Application layers it composes, plus
  `Game.CompositionRoot` and `VContainer`. Infrastructure and Presentation are
  not referenced until a fixture names their types.
- The architect owns the `.asmdef`; `coder-role` adds only test sources and
  their `.meta` files inside the assembly folder.

The one-assembly preference above is narrowed, not revoked: one PlayMode
assembly per owning assembly under test — Arena behavior in
`Game.Arena.PlayMode.Tests`, composition in
`Game.CompositionRoot.PlayMode.Tests`. No production assembly gains a
reference.

## Rationale

- One test-only assembly keeps the production dependency graph intact; the
  architecture boundary test ignores `.Tests` assemblies by design.
- Repository, Domain, and Application tests keep running in EditMode, where
  they are faster and need no play mode.
- Adapter and presentation tests share one harness, which also serves the
  future Unity clock, spawn, and lifecycle adapters (T-6.1, T-6.7, T-6.9).
- The `.Tests` suffix keeps the assembly inside the existing naming and
  boundary conventions.

## Consequences

### Positive

- PlayMode acceptance for F-001 and F-003 becomes testable in the Unity Test
  Runner.
- No production assembly, reference, or platform changes.
- One new assembly rather than two or three.

### Negative

- The test assembly references both Infrastructure and Presentation; it is
  the only place where those two layers meet, and only in tests.
- A single PlayMode assembly becomes the home of all play-mode tests, so it
  will grow with T-6 and T-7.

### Neutral / Risks

- `"includePlatforms": []` makes the assembly a PlayMode assembly; the
  `UNITY_INCLUDE_TESTS` constraint keeps it out of player builds.
- If PlayMode coverage later needs per-layer isolation, a follow-up ADR can
  split it.

## Alternatives

1. **Make `Game.Arena.Infrastructure.Tests` PlayMode-capable and add a
   `Game.Arena.Presentation` reference.**
   - Why not chosen: in-memory repository tests would move to the PlayMode
     runner, and one assembly would span two layers.
2. **Two assemblies — `Game.Arena.Infrastructure.PlayMode.Tests` and
   `Game.Arena.Presentation.Tests`.**
   - Why not chosen: more structure than the current test surface justifies.

## Compliance with paradigm

- ADR-0007: no production assembly gains a reference; only a `.Tests`
  assembly references Presentation, which the boundary test exempts.
- ADR-0015: the standalone build and `dotnet test` suites are untouched.
- ADR-0016: the assembly lives inside the Arena context folder, next to the
  other test assemblies.
- `docs/architect/paradigm.md`: no deviation; no exception is added.

## Migration plan

1. Authorize `coder-role` to create the `.asmdef` exactly as specified.
2. Move F-001 and F-003 PlayMode tests into `Game.Arena.PlayMode.Tests`.
3. Keep the existing Editor-only tests where they are.

## References

- `docs/architect/coder-inbox/c1.md` — the coder's deferral
- `docs/architect/coder-inbox/t8-1-composition-root.md` — the T-8.1 deferral
  behind the 2026-10-25 amendment
- `docs/coder/architect-inbox/F-001.md`, `docs/coder/architect-inbox/F-003.md`
- `docs/coder/delivery-plan.md` — T-6.10, T-7.7
- `Assets/_Project/Tests/Architecture/AssemblyBoundaryTests.cs` — test
  assembly exemption
