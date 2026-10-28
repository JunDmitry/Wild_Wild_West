namespace Game.Arena.Domain.Events
{
    /// <summary>
    /// A Domain event that can be refilled by the aggregate after its pool
    /// slot is reused. Implementation detail of the per-run event pool; it
    /// never appears in a public signature.
    /// </summary>
    internal interface IPooledArenaDomainEvent : IArenaDomainEvent
    {
        void Reset();
    }
}
