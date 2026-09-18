# Legacy Freeze Policy

## Prohibited Changes

The following changes are prohibited in legacy gameplay assemblies:

- new gameplay features;
- architecture refactoring;
- quality-only cleanup;
- defect fixes without approved hotfix procedure;
- performance optimization;
- public API expansion;
- new dependencies;
- moving legacy source files between assemblies.

## Allowed Changes

The following changes are allowed:

- a hotfix that blocks the current shipped game;
- a hotfix that blocks migration;
- a hotfix that blocks CI;
- a change required to compile after a Unity upgrade.

## Hotfix Procedure

1. Add or update an entry in the legacy defect register.
2. Add a regression test before changing production code.
3. Keep the code diff minimal.
4. Update characterization tests if observed behavior changed.
5. Update behavior traceability if a baseline scenario is affected.
6. Record the reason for the hotfix in the pull request.

Any legacy production change that does not follow this procedure must be rejected during review.