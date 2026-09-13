# ADR-0009: Freeze legacy gameplay code and define the migration boundary

- **Status:** Accepted
- **Date:** 2026-09-13
- **Authors:** Dmitry Rysev
- **Reviewers:** none
- **Related ADRs:** [ADR-0002](ADR-0002-adopt-iddd-arena-combat.md), [ADR-0003](ADR-0003-arena-run-aggregate-and-repository.md), [ADR-0007](ADR-0007-unity-isolation-and-layer-dependencies.md)

## Context

The project is migrating from the legacy Core.Model and Core.Rules implementation to the Arena Combat bounded context built with IDDD. During the migration both implementations exist in the repository. Without explicit rules, fixes and features would continue landing in legacy code, the two models could become coupled, and the migration would never converge.

Behavior baselines, characterization tests, defect reproduction tests, and assembly boundary tests already exist.

## Decision

Legacy gameplay assemblies are frozen. Only hotfixes that unblock the current game, the migration, or project compilation are allowed, and each hotfix requires a defect register entry and a regression test.

Legacy production assemblies must not reference Game.Arena assemblies. Game.Arena assemblies must not reference legacy assemblies. Both directions are enforced by compilation-graph tests.

No shared adapter assembly between the two models is introduced. Production switching is performed as a cut-over in the Composition Root after the switching criteria are met. Until then the shipped game runs entirely on the legacy stack, and the new model is exercised by tests and a development sandbox scene excluded from builds.

Switching criteria: full behavior traceability coverage, corrected-behavior tests for every reproduced defect, green architecture and domain test suites, green PlayMode smoke scenarios, and a frame-time comparison within an agreed threshold.

## Rationale

- Freezing legacy prevents divergence between the baseline and the target model.
- Bidirectional isolation keeps both dependency graphs verifiable.
- A cut-over avoids state converters between two gameplay models, which are error-prone and unnecessary for a single-player game without persisted gameplay data.
- Explicit switching criteria make the migration completion measurable.

## Consequences

### Positive

- Migration effort concentrates on the Arena Combat context.
- No hidden bridges between old and new models.
- Legacy removal in the final step is mechanical.

### Negative

- New gameplay features wait until the new model ships.
- The new model is not validated by the main scene until cut-over.

### Neutral / Risks

- A prolonged migration extends the freeze period.
- The Composition Root temporarily references legacy during the cut-over step and loses that reference at legacy removal.

## Alternatives

1. **Strangler migration with state adapters between models** — justified for systems with live data and external consumers.
   - Why not chosen: this game has no persisted gameplay state or external consumers; adapters would add risk without value.
2. **No freeze, parallel development in both models** — leads to double maintenance and baseline drift.
   - Why not chosen: characterization tests would chase a moving target.
3. **Immediate rewrite with deletion of legacy code** — removes the behavioral reference before the new model is complete.
   - Why not chosen: baseline equivalence would become unverifiable.

## References

- [Strangler Fig Application](https://martinfowler.com/bliki/StranglerFigApplication.html)
- [Vaughn Vernon, Implementing Domain-Driven Design](https://www.domainlanguage.com/ddd/reference/)