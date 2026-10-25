namespace Game.Arena.Application.Dispatch
{
    public enum ArenaRunDispatchStage
    {
        SnapshotPublication = 0,
        EntityViewSynchronization = 1,
        GameplayVisual = 2,
        Hud = 3,
        AudioAndVisualEffects = 4,
        AnalyticsAndDiagnostics = 5,
    }
}
