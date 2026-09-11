using Game.Core.Model.Configs;
using Game.Core.Model.Enums;
using Game.Core.Model.Results;
using Game.Core.Model.States;
using Game.Core.Rules.Builders;

namespace Game.Core.Rules
{
    public static class GameFlowRules
    {
        public static GameState ApplyPhaseTransitions(
            in GameState state,
            in GameConfig config,
            SimulationResultBuilder resultBuilder)
        {
            if (state.Phase == GamePhase.Playing && resultBuilder.PlayerDefeated)
            {
                resultBuilder.MarkGamePhase(GamePhase.Defeat);
                return WithPhase(state, GamePhase.Defeat, state.Time);
            }

            if (state.Phase == GamePhase.Playing && state.CurrentWave.Phase == WavePhase.Completed)
            {
                if (state.CurrentWave.Number >= config.Waves.Count)
                {
                    resultBuilder.MarkGamePhase(GamePhase.Victory);
                    return WithPhase(state, GamePhase.Victory, state.Time);
                }

                int nextNumber = state.CurrentWave.Number + 1;
                WaveState nextWave = WaveRules.StartNextWave(config, nextNumber);
                resultBuilder.MarkWaveStarted(nextNumber);

                return new GameState(
                    state.Phase,
                    state.Player,
                    state.Enemies,
                    nextWave,
                    state.Time,
                    state.PhaseEnteredTime);
            }

            return state;
        }

        public static GameState TickNonPlaying(
            in GameState state,
            in GameConfig config,
            float time,
            float delta,
            SimulationResultBuilder resultBuilder)
        {
            GameState advanced = new(
                state.Phase,
                state.Player,
                state.Enemies,
                state.CurrentWave,
                time,
                state.PhaseEnteredTime);

            if (state.Phase == GamePhase.Defeat)
            {
                float elapsed = time - state.PhaseEnteredTime;

                if (elapsed >= config.RestartDelay)
                {
                    resultBuilder.MarkRestartRequested();
                }
            }

            return advanced;
        }

        private static GameState WithPhase(in GameState state, in GamePhase phase, float enteredAt)
        {
            return new GameState(phase, state.Player, state.Enemies, state.CurrentWave, state.Time, enteredAt);
        }
    }
}
