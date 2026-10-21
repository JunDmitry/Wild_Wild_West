using System;
using Game.Arena.Domain.Combat;

namespace Game.Arena.Domain.Configuration
{
    public sealed class EnemyCatalog
    {
        private readonly EnemyDefinition _regular;
        private readonly EnemyDefinition _boss;

        public EnemyCatalog(EnemyDefinition regular, EnemyDefinition boss)
        {
            if (regular == null)
            {
                throw new ArgumentNullException(nameof(regular));
            }

            if (boss == null)
            {
                throw new ArgumentNullException(nameof(boss));
            }

            if (regular.Kind != EnemyKind.Regular)
            {
                throw new ArgumentException("Definition kind must be Regular.", nameof(regular));
            }

            if (boss.Kind != EnemyKind.Boss)
            {
                throw new ArgumentException("Definition kind must be Boss.", nameof(boss));
            }

            _regular = regular;
            _boss = boss;
        }

        public EnemyDefinition Get(EnemyKind kind)
        {
            return kind switch
            {
                EnemyKind.Regular => _regular,
                EnemyKind.Boss => _boss,
                _ => throw new ArgumentOutOfRangeException(nameof(kind)),
            };
        }
    }
}
