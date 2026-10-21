# Vision

The product's vision. Written once, revised only when a goal,
non-goal, or metric changes. This file arbitrates priority conflicts —
see `references/prioritization.md`.

## Vision

Wild-Wild West is a compact third-person arena shooter: a lone gunslinger holds a closed arena against three waves of enemies, switching between a firearm and a melee weapon, with a boss closing each wave. It is an educational, portfolio-scale project — a demonstration of a clean, domain-driven game core as much as a playable game — not a commercial product. Its mechanics follow the project brief exactly; they may be clarified, not replaced.

## Target user

- Players who enjoy short, self-contained wave-survival arena games.
- Reviewers and recruiters evaluating the project as an engineering portfolio piece.
- The developer as a learner exercising architecture and Unity delivery.

## Goals

- Deliver a complete, playable three-wave arena run that ends in an explicit Victory window.
- Keep the combat loop readable: move, aim, shoot or strike, take damage, die, restart.
- Demonstrate a domain-driven, engine-isolated core beneath the Unity presentation.
- Preserve the brief's mechanics exactly — no additional systems, weapons, or enemies.
- Keep the scope playable and reviewable in a portfolio context.

## Non-goals

- Campaign, narrative, cutscenes, or scripted encounters.
- Multiplayer, online services, or networking.
- Progression and persistence: XP, unlocks, saves, leaderboards.
- Weapons, abilities, or enemy types beyond the brief.
- Mobile or console releases; monetization.
- Inventing mechanics the brief does not describe (ammo, dodge, upgrades, and similar).

## Success metrics

- A player can complete all three waves, defeat each boss, and see "Victory" — with no crash.
- A player can die and observe an automatic scene restart with the wave counter reset.
- Both weapons are usable on demand and Q switches between them in one press.
- Every feature in `docs/product/backlog.md` traces to a described mechanic in the brief.
- A run is playable start to finish on the project's target Unity version.
