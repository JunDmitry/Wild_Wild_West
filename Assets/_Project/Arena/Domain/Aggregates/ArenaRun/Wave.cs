using System;
using Game.Arena.Domain.Configuration;
using Game.Arena.Domain.Identity;

namespace Game.Arena.Domain.Aggregates.ArenaRun
{
    internal sealed class Wave
    {
        public Wave(WaveDefinition definition)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            Number = definition.Number;
            Phase = WavePhase.RegularCombat;
            RegularEnemiesRemainingToSpawn = definition.RegularEnemyCount;
            BossStatus = BossStatus.NotSpawned;
        }

        public WaveNumber Number { get; }

        public WavePhase Phase { get; private set; }

        public int RegularEnemiesRemainingToSpawn { get; private set; }

        public BossStatus BossStatus { get; private set; }

        public bool IsRegularSpawnDue => Phase == WavePhase.RegularCombat && RegularEnemiesRemainingToSpawn > 0;

        public bool IsBossSpawnDue => Phase == WavePhase.BossCombat && BossStatus == BossStatus.NotSpawned;

        public void RecordRegularSpawn()
        {
            if (IsRegularSpawnDue == false)
            {
                throw new InvalidOperationException("Regular spawn is not due.");
            }

            RegularEnemiesRemainingToSpawn--;
        }

        public void RecordBossSpawn()
        {
            if (IsBossSpawnDue == false)
            {
                throw new InvalidOperationException("Boss spawn is not due.");
            }

            BossStatus = BossStatus.Alive;
        }

        public bool TryEnterBossCombat(int activeRegularEnemies)
        {
            if (Phase != WavePhase.RegularCombat)
            {
                return false;
            }

            if (RegularEnemiesRemainingToSpawn > 0)
            {
                return false;
            }

            if (activeRegularEnemies > 0)
            {
                return false;
            }

            Phase = WavePhase.BossCombat;
            return true;
        }

        public void MarkBossDefeated()
        {
            if (BossStatus != BossStatus.Alive)
            {
                throw new InvalidOperationException("Boss is not alive.");
            }

            BossStatus = BossStatus.Defeated;
        }
    }
}
