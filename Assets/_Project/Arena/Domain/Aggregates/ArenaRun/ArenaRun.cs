using System;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Identity;

namespace Game.Arena.Domain.Aggregates.ArenaRun
{
    public sealed class ArenaRun
    {
        internal ArenaRun(
            ArenaRunId id,
            ArenaRunStatus status,
            AggregateRevision revision)
        {
            if (id.IsNone)
            {
                throw new ArgumentException("ArenaRunId cannot be None.", nameof(id));
            }

            Id = id;
            Status = status;
            Revision = revision;
        }

        public ArenaRunId Id { get; }
        public ArenaRunStatus Status { get; private set; }
        public AggregateRevision Revision { get; private set; }
    }
}
