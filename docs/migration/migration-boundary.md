# Migration Boundary

---

## Dependency directions during migration

- Legacy production        → does not see the `Game.Arena.*`
- Game.Arena.*             → does not see the `legacy`
- Game.Legacy.*.Tests      → sees the `legacy`
- Game.Architecture.Tests  → does not refer to anyone, checks the graph

---

## Where is the transition code allowed

The only potential place where the two worlds connect is the Composition Root, and only at the stage of switching production flow (T-10). Before:

- `Game.CompositionRoot` does not reference legacy;
- there is no shared "common" assembly between legacy and `Game.Arena.*`;
- there is no copying legacy types to Domain "for a while".

---

## Intermediate check of the new model without switching

So that the new model is tested not only by unit tests up to T-10:

- A separate `ArenaSandbox` `dev-scene` (not included in the `build`).
- The new model's own `Composition Root`.
- `Smoke` scenarios: launch, movement, attack, wave, victory, defeat.

The main game scene continues to run on legacy until the switch criteria is completed.

---