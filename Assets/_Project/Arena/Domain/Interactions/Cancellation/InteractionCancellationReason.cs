namespace Game.Arena.Domain.Interactions
{
    public enum InteractionCancellationReason
    {
        None = 0,
        SupersededByLifecycle = 1,
        ExternalResolutionTimeout = 2,
        ArenaRunTerminated = 3,
        ApplicationShutdown = 4,
    }
}
