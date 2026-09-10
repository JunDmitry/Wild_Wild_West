using System.Collections.Generic;
using Game.Core.Model.Configs;
using Game.Core.Model.Entities;
using Game.Core.Model.States;

namespace Game.Core.Model.Initializers
{
    public static class GameInit
    {
        public static GameState NewGame(
            in GameConfig config,
            EntityId playerId,
            int waveNumber)
        {
            PlayerState player = PlayerInit.FromConfig(config, playerId);
            WaveState wave = WaveInit.FromConfig(config, waveNumber);

            return new GameState(
                phase: Enums.GamePhase.Playing,
                player,
                enemies: new Dictionary<EntityId, EnemyState>(),
                currentWave: wave,
                time: 0f);
        }
    }
}
