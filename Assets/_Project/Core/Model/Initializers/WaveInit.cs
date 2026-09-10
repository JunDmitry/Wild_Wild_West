using Game.Core.Model.Configs;
using Game.Core.Model.States;

namespace Game.Core.Model.Initializers
{
    public static class WaveInit
    {
        public static WaveState FromConfig(in GameConfig config, int waveNumber)
        {
            WaveConfig waveConfig = config.Waves[waveNumber - 1];

            return new WaveState(
                waveConfig.Number,
                Enums.WavePhase.RegularCombat,
                waveConfig.RegularCount,
                regularAlive: 0,
                bossSpawned: false,
                bossAlive: false);
        }
    }
}
