using System.Collections.Generic;
using Game.Arena.Application.ReadModels;
using Game.Arena.Application.Ticks;
using Game.Arena.Domain.Aggregates.ArenaRun;

namespace Game.Arena.Application.Tests.Ticks
{
    internal class ArenaRunTickResultFactory
    {
        private readonly ArenaRunSnapshotMapper _snapshotMapper;

        public ArenaRunTickResultFactory(ArenaRunSnapshotMapper snapshotMapper)
        {
            _snapshotMapper = snapshotMapper;
        }

        public ArenaRunTickResult Create(
            ArenaRun run,
            Domain.Concurrency.AggregateRevision finalRevision,
            IReadOnlyList<Domain.Events.IArenaDomainEvent> domainEvents,
            IReadOnlyList<ArenaRunTickStage> executedStages)
        {
            return new(run.Id, finalRevision, _snapshotMapper.Map(run.CreateSnapshot()), domainEvents, executedStages);
        }
    }
}
