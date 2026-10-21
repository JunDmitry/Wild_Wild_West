# ADR-0016: Organize gameplay by bounded context and documentation by role zone

- **Status:** Accepted
- **Date:** 2026-10-21
- **Deciders:** Dmitry Rysev
- **Related:** [ADR-0002](ADR-0002-adopt-iddd-arena-combat.md), [ADR-0009](ADR-0009-legacy-freeze-and-migration-boundary.md), [ADR-0015](ADR-0015-standalone-dotnet-build-for-arena-core.md), task `arch-init-1`

## Context

The project has one bounded context, Arena Combat, but its files sit at `Assets/_Project/Arena/` next to legacy and shared folders, and its tests sit in a separate `Assets/_Project/Tests/` tree. The documentation is split across `docs/adr/`, `docs/architecture/arena-combat/`, and `docs/migration/`, while the role workflow expects each role to own a zone under `docs/<role>/`. A second bounded context would force a second restructuring of every asmdef, namespace, and test path if the layout stays as it is.

The project is migrating from the legacy `Game.Core.*` stack (ADR-0009), which must stay in place and frozen. The layout must accommodate that legacy island without mixing it into the new context.

## Decision

> We will place each bounded context under `Assets/_Project/Contexts/<Context>/`, keep its tests beside it under `Contexts/<Context>/Tests/`, and organize documentation by owning role zone: `docs/architect/`, `docs/analyst/`, `docs/qa/`, and `docs/coder/`.

Consequences:

- Arena Combat lives at `Assets/_Project/Contexts/Arena/` with `Domain`, `Application`, `Infrastructure`, `Presentation`, and `Tests`.
- The three Arena test assemblies move under `Contexts/Arena/Tests/`; `Tests/Architecture` and `Tests/LegacyCharacterization` remain project-level under `Assets/_Project/Tests/`.
- Assembly names, GUIDs, and all `Game.Arena.*` namespaces stay unchanged; the move is physical only.
- Documentation moves to the owning role zone: architecture records, ADRs, and migration policy to `docs/architect/`; ubiquitous language and domain invariants to `docs/analyst/`; behavior baseline, defect register, and traceability to `docs/qa/`; the delivery plan to `docs/coder/`.
- The legacy `Game.Core.Model` / `Game.Core.Rules` stack stays where it is, frozen, until T-10.
- Explicitly out of scope: introducing a second bounded context now, renaming namespaces, and retiring legacy code.

## Options considered

### Option A — One Arena context under `Contexts/`, docs by role zone (chosen)
- Pros: adding a second context later is a sibling folder; the role zones match the workflow; the legacy island stays separate; one move now.
- Cons: asmdef paths, test paths, and documentation links must be updated once; the delivery plan's paths change.
- Why chosen: it satisfies the training goal and the role model without inventing contexts the code does not support.

### Option B — Keep Arena at the top level and move only the docs
- Pros: no code move today.
- Cons: the root mixes the bounded context with shared and legacy folders; a later move touches every asmdef and test path anyway.
- Why not chosen: it defers the structural debt instead of removing it.

### Option C — Split into multiple contexts now
- Pros: a multi-context skeleton immediately.
- Cons: adds contexts the code and requirements do not support; expands the migration beyond T-4.
- Why not chosen: no second context has a model, a language, or a delivery stage yet.

### Option D — No folder or docs move
- Pros: zero churn.
- Cons: does not address the request; the architect zone does not own the ADRs it must follow.
- Why not chosen: it leaves the architecture workspace incomplete.

## Consequences

### Positive

- Context boundaries are visible in the folder tree.
- Tests live beside the code they verify.
- Each role has one documentation zone to read and write.
- A future context is a sibling, not a restructuring.

### Negative / trade-offs

- The move is a large path-only diff; reviewers must check that asmdef references and namespaces did not change.
- Relative links between moved documents need updating.
- The delivery plan and any external bookmarks to old paths must be updated.

### Neutral

- Unity regenerates the root `.csproj` files and `Wild_Wild_West.sln` on the next Editor open; those files are not hand-edited.

## Compliance with paradigm

- Supports the bounded-context boundary and the role-zone documentation rule in `docs/architect/paradigm.md`.
- No deviation; no exception entry is needed. Legacy placement remains the ADR-0009 exception.

## Migration plan

1. Move `Assets/_Project/Arena` to `Assets/_Project/Contexts/Arena`, including `.meta` files.
2. Move the three Arena test folders to `Contexts/Arena/Tests/`.
3. Leave `Tests/Architecture` and `Tests/LegacyCharacterization` in place.
4. Move the documentation into the role zones and update the one relative link between the delivery plan and the tick contract.
5. Run the standalone .NET tests and the Unity test suites.

Each step leaves both build paths green.

## References

- Related code: `Assets/_Project/Contexts/Arena`, `Assets/_Project/Tests`
- Related docs: `docs/architect/paradigm.md`, `docs/architect/adr/ADR-0009-legacy-freeze-and-migration-boundary.md`
- External sources: [Vaughn Vernon, Implementing Domain-Driven Design](https://www.domainlanguage.com/ddd/reference/)
