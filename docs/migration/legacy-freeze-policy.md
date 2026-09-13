# Legacy Freeze Policy

---

## Banned in the legacy code

- New functionality.
- Refactoring for the sake of quality.
- Correction of defects from the `defect register`.
- Performance optimization.
- Renaming and moving files between builds.
- Adding new public `API`s.
- New dependencies.

---

## Allowed in the legacy code

- A `hotfix` that blocks the launch of the current game.
- A `hotfix` that blocks migration or `CI`.
- The change required for compilation when updating `Unity`.

---

## The hotfix procedure

1. An entry is being created in the defective register with the `Hotfix` status.
2. A regression test is added before the correction.
3. The fix is minimal by diff.
4. Characterization-the tests are updated if the fixed behavior has changed.
5. The traceability matrix has an impact on baseline scenarios.

Any legacy change without this procedure is rejected for review.

---