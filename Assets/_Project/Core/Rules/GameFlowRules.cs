using System;
using Game.Core.Model.Configs;
using Game.Core.Model.Enums;
using Game.Core.Model.States;
using Game.Core.Rules.Outcomes;

namespace Game.Core.Rules
{
    public static class GameFlowRules
    {
        public static GameFlowOutcome ApplyPhaseTransitions(
            in GameState state,
            in GameConfig config,
            bool playerDefeated)
        {
            if (state.Phase == GamePhase.Playing && playerDefeated)
            {
                GameState defeatState = WithPhase(state, GamePhase.Defeat, state.Time);

                return new GameFlowOutcome(
                    defeatState,
                    gamePhaseChanged: true,
                    defeatState.Phase,
                    waveStarted: false,
                    waveNumber: 0,
                    restartRequested: false);
            }

            if (state.Phase == GamePhase.Playing && state.CurrentWave.Phase == WavePhase.Completed)
            {
                if (state.CurrentWave.Number >= config.Waves.Count)
                {
                    GameState victoryState = WithPhase(state, GamePhase.Victory, state.Time);

                    return new GameFlowOutcome(
                        victoryState,
                        gamePhaseChanged: true,
                        victoryState.Phase,
                        waveStarted: false,
                        waveNumber: 0,
                        restartRequested: false);
                }

                int nextNumber = state.CurrentWave.Number + 1;
                WaveState nextWave = WaveRules.StartNextWave(config, nextNumber);

                GameState nextWaveState = new(
                    state.Phase,
                    state.Player,
                    state.Enemies,
                    nextWave,
                    state.Time,
                    state.PhaseEnteredTime);

                return new GameFlowOutcome(
                    nextWaveState,
                    gamePhaseChanged: false,
                    newGamePhase: state.Phase,
                    waveStarted: true,
                    nextWave.Number,
                    restartRequested: false);
            }

            return new(
                state,
                gamePhaseChanged: false,
                newGamePhase: state.Phase,
                waveStarted: false,
                waveNumber: 0,
                restartRequested: false);
        }

        public static GameFlowOutcome TickNonPlaying(
            in GameState state,
            in GameConfig config,
            float time)
        {
            GameState advanced = new(
                state.Phase,
                state.Player,
                state.Enemies,
                state.CurrentWave,
                time,
                state.PhaseEnteredTime);
            bool restartRequested = false;

            if (state.Phase == GamePhase.Defeat)
            {
                float elapsed = time - state.PhaseEnteredTime;

                if (elapsed >= config.RestartDelay)
                {
                    restartRequested = true;
                }
            }

            return new(
                advanced,
                gamePhaseChanged: false,
                newGamePhase: state.Phase,
                waveStarted: false,
                waveNumber: 0,
                restartRequested);
        }

        private static GameState WithPhase(in GameState state, in GamePhase phase, float enteredAt)
        {
            return new GameState(phase, state.Player, state.Enemies, state.CurrentWave, state.Time, enteredAt);
        }
    }
}
