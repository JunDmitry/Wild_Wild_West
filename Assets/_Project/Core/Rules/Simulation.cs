using System.Collections.Generic;
using Game.Core.Model.Configs;
using Game.Core.Model.Entities;
using Game.Core.Model.Enums;
using Game.Core.Model.Results;
using Game.Core.Model.States;
using Game.Core.Rules.Builders;

namespace Game.Core.Rules
{
    public static class Simulation
    {
        public static SimulationOutput Tick(
            GameState state,
            in GameConfig config,
            in FrameInput input,
            in FrameContext context,
            float delta)
        {
            SimulationResultBuilder resultBuilder = new();
            float time = state.Time + delta;

            if (state.Phase != GamePhase.Playing)
            {
                GameState idle = GameFlowRules.TickNonPlaying(
                    state,
                    config,
                    time,
                    delta,
                    resultBuilder);

                return new(idle, resultBuilder.Build());
            }

            PlayerState player = state.Player;
            IReadOnlyDictionary<EntityId, EnemyState> enemies = state.Enemies;
            Dictionary<EntityId, EnemyState> nextEnemies;
            WaveState wave = state.CurrentWave;

            player = WeaponRules.TrySwitch(
                player,
                input.SwitchWeaponPressed,
                resultBuilder);

            player = MovementRules.MovePlayer(
                player,
                input,
                context,
                config.Arena,
                config.Player.MoveSpeed,
                delta);

            (player, enemies) = WeaponRules.TryPlayerAttack(
                player,
                enemies,
                input,
                context,
                config,
                time,
                resultBuilder);

            (player, nextEnemies) = EnemyRules.TickAll(
                player,
                enemies,
                config,
                time,
                delta,
                resultBuilder);

            enemies = nextEnemies;

            (player, nextEnemies, wave) = DeathRules.Apply(
                player,
                enemies,
                wave,
                resultBuilder);

            enemies = nextEnemies;

            (wave, nextEnemies) = WaveRules.Tick(
                wave,
                enemies,
                config,
                context,
                resultBuilder);

            enemies = nextEnemies;

            GameState mid = new(
                GamePhase.Playing,
                player,
                enemies,
                wave,
                time,
                state.PhaseEnteredTime);

            GameState finalState = GameFlowRules.ApplyPhaseTransitions(
                mid,
                config,
                resultBuilder);

            return new SimulationOutput(finalState, resultBuilder.Build());
        }
    }
}
