# Architecture Decision Records for Wild_Wild_West

> ADR (Architecture Decision Records) is a log of significant architectural decisions for the project.  
> Each ADR captures the context, the decision itself, its rationale, and its consequences.

## Document Metadata

- **Project:** `Wild_Wild_West`
- **Owner:** `Dmitry Rysev`
- **Repository:** `https://github.com/JunDmitry/Wild_Wild_West`
- **Created:** `2026-09-13`
- **Last updated:** `2026-09-13`

## Purpose

This document contains the index and template for the project's architectural decisions.  
ADRs are stored as Markdown files in the `docs/adr/` directory so that decisions are versioned, reviewed through code review, and kept close to the source code.

## Conventions

- One ADR — one architecturally significant decision.
- Accepted ADRs are not rewritten. A change in decision is recorded as a new ADR or a status change.
- Identifiers: `ADR-0001`, `ADR-0002`, `ADR-0003`, ...
- File name: `ADR-XXXX-short-title.md`.
- Statuses: `Proposed`, `Accepted`, `Rejected`, `Deprecated`, `Superseded by ADR-XXXX`.
- Documentation language: English.
- ADRs are not required for small local decisions.

## Decision Index

| ID | Title | Status | Date | Link |
|---|---|---|---|---|
| ADR-0000 | Use ADRs to document architectural decisions | Accepted | YYYY-MM-DD | [ADR-0000](ADR.md) |
| ADR-0001 | Independent identity per type | Accepted | 2026-09-13 | [ADR-0001](ADR-0001-Independent_identity_per_type.md) |
| ADR-0002 | Adopt iddd arena combat | Accepted | 2026-09-13 | [ADR-0002](ADR-0002-adopt-iddd-arena-combat.md) |
| ADR-0003 | Arena run aggregate and repository | Accepted | 2026-09-13 | [ADR-0003](ADR-0003-arena-run-aggregate-and-repository.md) |
| ADR-0004 | Typed process wide identity sources | Accepted | 2026-09-13 | [ADR-0004](ADR-0004-typed-process-wide-identity-sources.md) |
| ADR-0005 | Domain events and application notifications | Accepted | 2026-09-13 | [ADR-0005](ADR-0005-domain-events-and-application-notifications.md) |
| ADR-0006 | Sequential external interaction protocol | Accepted | 2026-09-13 | [ADR-0006](ADR-0006-sequential-external-interaction-protocol.md) |
| ADR-0007 | Unity isolation and layer dependencies | Accepted | 2026-09-13 | [ADR-0007](ADR-0007-unity-isolation-and-layer-dependencies.md) |
| ADR-0008 | Code style and static usage policy | Accepted | 2026-09-13 | [ADR-0008](ADR-0008-code-style-and-static-usage-policy.md) |
| ADR-0009 | Legacy freeze and migration boundary | Accepted | 2026-09-13 | [ADR-0009](ADR-0009-legacy-freeze-and-migration-boundary.md) |
| ADR-0010 | Correlate domain interactions with aggregate revision | Accepted | 2026-09-14 | [ADR-0010](ADR-0010-correlate-domain-interactions-with-aggregate.md) |

## New ADR Template

```md
# ADR-XXXX: <Short decision title>

- **Status:** Proposed
- **Date:** YYYY-MM-DD
- **Authors:** <names>
- **Reviewers:** <names>
- **Related ADRs:** <ADR-XXXX or none>

## Context

Describe the problem, constraints, requirements, and current situation.

## Decision

State the decision in affirmative form.

## Rationale

Why this decision was chosen. Which criteria mattered.

## Consequences

### Positive

- ...

### Negative

- ...

### Neutral / Risks

- ...

## Alternatives

1. **<Alternative 1>** — description.
   - Why not chosen: ...
2. **<Alternative 2>** — description.
   - Why not chosen: ...

## References

- <link>
```

---

# ADR-0001: Use ADRs to document architectural decisions

- **Status:** Accepted
- **Date:** YYYY-MM-DD
- **Authors:** `<names>`
- **Reviewers:** `<names>`
- **Related ADRs:** none

## Context

The project makes architectural decisions that affect the team, code, infrastructure, and future product development. Without recording the context and reasons behind such decisions, new contributors spend time reconstructing the rationale, and already accepted decisions may be accidentally violated.

## Decision

We will maintain ADRs as Markdown files in the `docs/adr` directory.  
Each significant architectural decision will be documented as a separate ADR using a consistent template.  
Accepted ADRs are considered immutable: changes are recorded as a new ADR or a status change.

## Rationale

- ADRs are stored next to the code and can go through review in the same process as changes.
- Markdown is convenient for reading, diffing, searching, and generating documentation.
- A consistent format simplifies onboarding and reduces the cost of discussion.
- The history of decisions becomes transparent to the team and stakeholders.

## Consequences

### Positive

- The history of architectural decisions is preserved.
- Onboarding new developers is faster.
- The risk of re-discussing already accepted decisions is reduced.
- Decisions can be linked to tasks, PRs, and documentation.

### Negative

- Discipline is required: ADRs must be created and updated in a timely manner.
- For small decisions, an ADR may be excessive.

### Neutral / Risks

- ADRs may become outdated if statuses are not maintained.
- With many ADRs, an up-to-date index is needed.
- The team must agree on which decisions are considered architecturally significant.

## Alternatives

1. **Keep decisions only in the Wiki** — worse connection to code and change history.
2. **Do not document architectural decisions** — leads to lost context and repeated discussions.
3. **Use a specialized ADR service** — excessive for most projects and complicates access.

## References

- [Architecture Decision Records](https://adr.github.io/)
- [Michael Nygard. Documenting Architecture Decisions](https://cognitect.com/blog/2011/11/15/documenting-architecture-decisions)
