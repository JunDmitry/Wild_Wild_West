using System.Collections.Generic;
using Game.Arena.Application.ReadModels;
using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Events;
using Game.Arena.Domain.Identity;

namespace Game.Arena.Application.Ticks
{
    internal sealed class ArenaRunTickRecorder
    {
        private readonly List<IArenaDomainEvent> _events;
        private readonly List<ArenaRunTickStage> _stages;

        public ArenaRunTickRecorder()
        {
            _events = new();
            _stages = new();
        }

        public int EventCount => _events.Count;

        public void RecordStage(ArenaRunTickStage stage)
        {
            _stages.Add(stage);
        }

        public void RecordChange(ArenaRunChange change)
        {
            if (change == null)
            {
                throw new System.ArgumentNullException(nameof(change));
            }

            int count = change.DomainEvents.Count;

            for (int i = 0; i < count; i++)
            {
                _events.Add(change.DomainEvents[i]);
            }
        }

        public ArenaRunTickResult Build(
            ArenaRunId arenaRunId,
            AggregateRevision finalRevision,
            ArenaRunSnapshot snapshot)
        {
            if (snapshot == null)
            {
                throw new System.ArgumentNullException(nameof(snapshot));
            }

            return new ArenaRunTickResult(
                arenaRunId,
                finalRevision,
                snapshot,
                _events,
                _stages);
        }
    }
}
