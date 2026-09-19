using System.Collections.Generic;
using Game.Arena.Application.Ports;
using Game.Arena.Domain.Identity;
using Game.Arena.Domain.Interactions.Attack;

namespace Game.Arena.Application.Tests.Ticks
{
    internal sealed class ScriptedTargetingResolver : IPlayerAttackTargetingResolver
    {
        private readonly List<EnemyId> _hits = new();

        public bool ReturnNull { get; set; }

        public int CallCount { get; private set; }

        public PlayerAttackImpactRequest LastRequest { get; private set; }

        public void SetHits(params EnemyId[] hits)
        {
            _hits.Clear();
            _hits.AddRange(hits);
        }

        public IReadOnlyList<EnemyId> Resolve(PlayerAttackImpactRequest request)
        {
            CallCount++;
            LastRequest = request;

            if (ReturnNull)
            {
                return null;
            }

            return _hits.ToArray();
        }
    }
}
