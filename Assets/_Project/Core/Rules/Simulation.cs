using System;
using System.Collections.Generic;
using Game.Core.Model.Configs;
using Game.Core.Model.Entities;
using Game.Core.Model.Enums;
using Game.Core.Model.Facts;
using Game.Core.Model.Results;
using Game.Core.Model.Simulation.Queries;
using Game.Core.Model.Simulation.Resolutions;
using Game.Core.Model.States;
using Game.Core.Rules.Builders;
using Game.Core.Rules.Outcomes;
using Game.Core.Rules.Planning;

namespace Game.Core.Rules
{
    public static class Simulation
    {
        public static SimulationPlan Plan(
            in GameState state,
            in GameConfig config,
            in FrameInput input,
            float delta)
        {
            return SimulationPlanner.Plan(state, config, input, delta);
        }

        public static SimulationOutput Execute(
            in GameState state,
            in GameConfig config,
            in FrameInput input,
            in SimulationPlan plan,
            in ResolvedSimulationContext context,
            float delta)
        {
            SimulationResultBuilder resultBuilder = new();
            float targetTime = state.Time + delta;

            if (state.Phase != GamePhase.Playing)
            {
                GameFlowOutcome idle = GameFlowRules.TickNonPlaying(
                    state,
                    config,
                    targetTime);

                return new SimulationOutput(idle.State, SimulationResult.Idle(state.Player.SelectedWeapon, idle.RestartRequested));
            }

            WeaponSwitchOutcome switchOutcome = WeaponRules.TrySwitch(state.Player, input.SwitchWeaponPressed);
            PlayerState player = state.Player;

            MovementOutcome movementOutcome = MovementRules.ApplyMovement(
                player,
                plan.HasMovementQuery,
                plan.Movement,
                context.Movement);
            player = movementOutcome.Player;

            AttackExecutionOutcome attackOutcome = WeaponRules.TryPlayerAttack(
                player,
                state.Enemies,
                plan.Attack,
                context.Attack,
                config,
                targetTime);
            player = attackOutcome.Player;
            IReadOnlyDictionary<EntityId, EnemyState> enemies = attackOutcome.Enemies;

            EnemyTickOutcome enemyOutcome = EnemyRules.TickAll(
                player,
                enemies,
                config,
                targetTime,
                delta);
            player = enemyOutcome.Player;
            enemies = enemyOutcome.Enemies;

            DeathResolutionOutcome deathOutcome = DeathRules.Apply(
                player,
                enemies,
                state.CurrentWave);
            player = deathOutcome.Player;
            enemies = deathOutcome.Enemies;
            WaveState wave = deathOutcome.Wave;

            WaveExecutionOutcome waveOutcome = WaveRules.Tick(
                wave,
                enemies,
                config,
                context.Spawn);
            wave = waveOutcome.Wave;
            enemies = waveOutcome.Enemies;

            GameState intermediate = new(
                GamePhase.Playing,
                player,
                enemies,
                wave,
                targetTime,
                state.PhaseEnteredTime);

            GameFlowOutcome flowOutcome = GameFlowRules.ApplyPhaseTransitions(
                intermediate,
                config,
                deathOutcome.PlayerDefeated);

            SimulationResult result = AggregateResult(
                switchOutcome,
                attackOutcome,
                enemyOutcome,
                deathOutcome,
                waveOutcome,
                flowOutcome);

            return new(flowOutcome.State, result);
        }

        private static SimulationResult AggregateResult(
            WeaponSwitchOutcome switchOutcome,
            AttackExecutionOutcome attackOutcome,
            EnemyTickOutcome enemyOutcome,
            DeathResolutionOutcome deathOutcome,
            WaveExecutionOutcome waveOutcome,
            GameFlowOutcome flowOutcome)
        {
            EnemySpawnedFact[] spawnedFacts = waveOutcome.EnemySpawned
                            ? new[] { waveOutcome.SpawnedFact }
                            : Array.Empty<EnemySpawnedFact>();

            SimulationResult result = new(
                switchOutcome.Switched,
                switchOutcome.NewWeapon,
                attackOutcome.AttackExecuted,
                attackOutcome.WeaponKind,
                attackOutcome.AnyHit,
                enemyOutcome.PlayerDamageTaken,
                deathOutcome.PlayerDefeated,
                spawnedFacts,
                attackOutcome.DamagedFacts,
                deathOutcome.DefeatedEnemyIds,
                waveOutcome.PhaseChanged,
                waveOutcome.NewPhase,
                flowOutcome.WaveStarted,
                flowOutcome.WaveNumber,
                flowOutcome.GamePhaseChanged,
                flowOutcome.NewGamePhase,
                flowOutcome.RestartRequested);

            return result;
        }
    }
}
