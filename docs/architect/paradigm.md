# Project Paradigm

## Primary paradigm

Domain-Driven Design in the style of *Implementing Domain-Driven Design* (IDDD).

## Secondary / complementary

- Clean Architecture layering: Domain, Application, Infrastructure, Presentation, CompositionRoot.
- Ports & Adapters: Application declares ports; Unity adapters implement them in Infrastructure.
- Compile-time Unity isolation: Domain and Application never reference `UnityEngine` or `UnityEditor` (ADR-0007).
- A standalone .NET build of the core proves the core builds and tests without Unity (ADR-0015).

## Layer rules

- Domain → no dependency on any other layer; no Unity types.
- Application → depends on Domain; declares ports; no Unity types.
- Infrastructure → depends on Application and Domain; implements ports with Unity APIs.
- Presentation → depends on Application; renders snapshots and consumes notifications; never references Infrastructure directly.
- CompositionRoot → depends on all four layers; the only place concrete dependencies are assembled.
- Legacy `Game.Core.Model` / `Game.Core.Rules` → frozen; no references to or from `Game.Arena.*` (ADR-0009).

## Boundaries

- One bounded context: Arena Combat. It owns `ArenaRun`, `Player`, `Enemy`, `Wave`, combat rules, domain events, and aggregate consistency.
- Bounded contexts live under `Assets/_Project/Contexts/<Context>/` (ADR-0016).
- Cross-context communication, when a second context exists, goes through Application contracts — never through Domain types.
- Documentation is organized by role zone: `docs/architect/`, `docs/analyst/`, `docs/qa/`, `docs/coder/` (ADR-0016).

## Exceptions

- The legacy stack remains in place until T-9/T-10 as an explicit migration exception, approved in ADR-0009.
- `Game.Performance` is a development-only measurement assembly compiled only under `PERF_MEASUREMENT` (ADR-0034). It sits outside the shipping layer graph, references CompositionRoot and Infrastructure, and is never referenced by a production assembly.

## History

- 2026-10-21 — initial record: IDDD primary with Clean Architecture and Ports & Adapters; standalone .NET core build and bounded-context layout recorded in ADR-0015 and ADR-0016.
