using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Identity;

namespace Game.Arena.Domain.Events
{
    public interface IDomainEvent
    {
        ArenaRunId ArenaRunId { get; }
        AggregateRevision AggregateRevision { get; }
    }
}
