using Game.Arena.Application.Hud;
using Game.Arena.Application.ReadModels;

namespace Game.Arena.Application.Dispatch
{
    /// <summary>
    /// A presentation handler that needs both the published snapshot and the
    /// tick's HUD model (ADR-0024, ADR-0027 amendment). The pair is what the
    /// HUD regions render; neither value is formatted here.
    /// </summary>
    public interface IArenaRunHudHandler
    {
        void Handle(ArenaRunSnapshot snapshot, ArenaRunHudSnapshot hud);
    }
}
