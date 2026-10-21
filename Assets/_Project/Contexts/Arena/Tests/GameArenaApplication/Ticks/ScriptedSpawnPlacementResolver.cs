using System.Collections.Generic;
using Game.Arena.Application.Ports;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Interactions.Spawn;

namespace Game.Arena.Application.Tests.Ticks
{
    internal sealed class ScriptedSpawnPlacementResolver : IEnemySpawnPlacementResolver
    {
        private readonly List<EnemySpawnRequest> _requests = new();

        public ScriptedSpawnPlacementResolver()
        {
            Position = new Position3D(22f, 0f, 0f);
        }

        public Position3D Position { get; set; }

        public int CallCount => _requests.Count;

        public IReadOnlyList<EnemySpawnRequest> Requests => _requests;

        public Position3D ResolveSpawnPosition(EnemySpawnRequest request)
        {
            _requests.Add(request);
            return Position;
        }
    }
}
