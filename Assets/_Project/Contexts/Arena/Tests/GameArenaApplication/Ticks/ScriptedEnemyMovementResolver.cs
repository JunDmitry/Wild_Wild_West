using System;
using System.Collections.Generic;
using Game.Arena.Application.Ports;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Interactions.Movement;

namespace Game.Arena.Application.Tests.Ticks
{
    internal sealed class ScriptedEnemyMovementResolver : IEnemyMovementResolver
    {
        public enum ResolveMode
        {
            AcceptRequested = 0,
            KeepInPlace = 1,
            OmitFirstEntry = 2,
            ReturnNull = 3,
        }

        public ResolveMode Mode { get; set; }

        public int CallCount { get; private set; }

        public EnemyMovementBatchRequest LastRequest { get; private set; }

        private readonly List<EnemyMovementBatchResolutionEntry> _entries = new();

        public IReadOnlyList<EnemyMovementBatchResolutionEntry> Resolve(
            EnemyMovementBatchRequest request,
            ReadOnlySpan<EnemyFootprint> occupied)
        {
            CallCount++;
            LastRequest = request;

            if (Mode == ResolveMode.ReturnNull)
            {
                return null;
            }

            _entries.Clear();

            for (int index = 0; index < request.Intents.Count; index++)
            {
                if (Mode == ResolveMode.OmitFirstEntry && index == 0)
                {
                    continue;
                }

                EnemyMovementIntent intent = request.Intents[index];

                Position3D position = Mode == ResolveMode.KeepInPlace
                    ? intent.Intent.From
                    : intent.Intent.RequestedPosition;

                _entries.Add(new EnemyMovementBatchResolutionEntry(intent.EnemyId, position));
            }

            return _entries;
        }
    }
}
