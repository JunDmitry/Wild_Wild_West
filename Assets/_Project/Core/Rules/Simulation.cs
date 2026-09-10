using System.Collections.Generic;
using Game.Core.Model.Configs;
using Game.Core.Model.Entities;
using Game.Core.Model.Enums;
using Game.Core.Model.Results;
using Game.Core.Model.States;

namespace Game.Core.Rules
{
    public static class Simulation
    {
        public static SimulationOutput Tick(
            GameState state,
            in GameConfig config,
            in FrameInput input,
            in FrameContext context,
            EntityId nextEntityId,
            float delta)
        {
            SimulationResult result = new();
            float time = state.Time + delta;

            if (state.Phase != GamePhase.Playing)
            {
                GameState idle = GameFlowRules.TickNonPlaying(state, config, time, delta, result);
                return new(idle, result, nextEntityId);
            }

            PlayerState player = state.Player;
            IReadOnlyDictionary<EntityId, EnemyState> enemies = state.Enemies;
            Dictionary<EntityId, EnemyState> nextEnemies;
            WaveState wave = state.CurrentWave;
            EntityId nextId = nextEntityId;

            player = WeaponRules.TrySwitch(player, input.SwitchWeaponPressed, result);
            player = MovementRules.MovePlayer(
                player, input, context, config.Arena, config.Player.MoveSpeed, delta);

            (player, enemies) = WeaponRules.TryPlayerAttack(
                player, enemies, input, context, config, time, result);
            (player, nextEnemies) = EnemyRules.TickAll(
                player, enemies, config, time, delta, result);
            enemies = nextEnemies;

            (player, nextEnemies, wave) = DeathRules.Apply(player, enemies, wave, result);
            enemies = nextEnemies;

            (wave, nextEnemies, nextId) = WaveRules.Tick(wave, enemies, config, context, nextId, result);
            enemies = nextEnemies;

            GameState mid = new(
                GamePhase.Playing,
                player,
                enemies,
                wave,
                time,
                state.PhaseEnteredTime);
            GameState finalState = GameFlowRules.ApplyPhaseTransitions(mid, config, result);

            return new SimulationOutput(finalState, result, nextId);
        }
    }
}
