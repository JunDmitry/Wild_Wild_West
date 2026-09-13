# ADR-0008: Enforce StyleCop, explicit code structure, and restricted static usage

- **Status:** Accepted
- **Date:** 2026-09-13
- **Authors:** project team
- **Reviewers:** none
- **Related ADRs:** ADR-0001, ADR-0007

## Context

The project requires consistent code style, explicit control flow, and readable architecture. The project also prohibits explanatory comments inside source code while requiring StyleCop compliance.

Unrestricted static classes make dependencies implicit and complicate testing, composition, replacement, and lifecycle management.

## Decision

The project will enforce StyleCop-compatible naming, formatting, member ordering, access modifiers, and brace rules.

StyleCop documentation rules requiring XML comments in C# source files will be disabled.

All conditional statements, loops, and methods will use explicit bodies.

Static classes and static methods will be used only for constants, immutable sentinel values, and stateless pure helpers where instance dependencies would provide no value.

Domain policies, factories, repositories, application services, dispatchers, adapters, and aggregate collaborators will be instance-based.

## Rationale

- Consistent structure reduces review and maintenance cost.
- Explicit bodies make changes safer and clearer.
- Disabling XML documentation rules respects the no-comments-in-code rule.
- Instance-based collaborators make dependencies explicit and testable.
- Restricting static usage prevents hidden global behavior.

## Consequences

### Positive

- Code style is automatically enforceable.
- Constructors reveal dependencies clearly.
- Domain policies and services can be substituted in tests.
- No architectural explanation is hidden in stale code comments.

### Negative

- More small classes and dependency registrations are required.
- Source code does not provide XML documentation tooltips.
- Developers must maintain Markdown architecture documentation and ADRs.

### Neutral / Risks

- Pure mathematical helpers may remain static when justified.
- Static convenience APIs must be reviewed carefully.
- StyleCop configuration requires maintenance when new rules are enabled.

## Alternatives

1. **Allow unrestricted static helper classes** — hides dependencies and encourages procedural architecture.
   - Why not chosen: the project is moving toward explicit IDDD collaborators.
2. **Require XML documentation for all public members** — conflicts with the no-comments-in-code rule.
   - Why not chosen: architecture documentation will live in ADR and Markdown files.
3. **Rely on manual style review only** — inconsistent results and unnecessary review cost.
   - Why not chosen: automated analyzers can enforce most style rules.

## References

- [StyleCop Analyzers](https://github.com/DotNetAnalyzers/StyleCopAnalyzers)
- [Architecture Decision Records](https://adr.github.io/)