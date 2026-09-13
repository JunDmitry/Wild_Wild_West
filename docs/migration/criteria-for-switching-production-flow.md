# Criteries

## Functional equivalence

- Each scenario is `B-001`..`B-011` is covered by the new model test.
- Each `LEG` defect with the `Reproduced` status has a test of new correct behavior in the `Arena` model.
- `Traceability matrix` does not contain empty cells in the `Future test` column.

## Quality

- `Architecture tests` are green.
- `Domain` and `Application` tests of the new model are green.
- `PlayMode smoke` on `ArenaSandbox` green:
   - Launch → Wave 1 → boss → Wave 2 → Wave 3 → `Victory`
   - launch → death → delay → `reload` → wave 1
   
## Performance

> Frame metering on the target scene with the maximum number of enemies
> from `WaveCatalog`: the new model is no worse than `legacy` by more than an
> agreed threshold. The threshold is fixed before `T-10`.

## Procedure

1. The main stage is transferred to the new `Composition Root`.
2. `Legacy`-the path remains in the repository for one release cycle.
3. After confirmation — `T-11`: delete `legacy` builds, `legacy` tests, `characterization/defect` tests, updating the `defect register` to `Resolved by migration`.