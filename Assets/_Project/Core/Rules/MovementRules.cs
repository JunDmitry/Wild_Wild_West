using Game.Core.Model.Configs;
using Game.Core.Model.Entities;
using Game.Core.Model.Simulation.Queries;
using Game.Core.Model.Simulation.Resolutions;
using Game.Core.Model.States;
using Game.Core.Rules.Mathematics;
using Game.Core.Rules.Outcomes;

namespace Game.Core.Rules
{
    public static class MovementRules
    {
        public static MovementOutcome MovePlayer(
            in PlayerState player,
            bool hasMovementQuery,
            in MovementQuery plannedMovement,
            in MovementResolution resolution)
        {
            if (hasMovementQuery == false || resolution.IsBlocked)
            {
                return new MovementOutcome(player);
            }

            PlayerState updated = new(
                player.Id,
                plannedMovement.To,
                player.CurrentHealth,
                player.MaxHealth,
                player.SelectedWeapon,
                player.RangedReadyTime,
                player.MeleeReadyTime);

            return new(updated);
        }

        public static Position3D ClampToArena(Position3D position, ArenaConfig arena)
        {
            float x = Math3D.Clamp(position.X, arena.MinX, arena.MaxX);
            float z = Math3D.Clamp(position.Z, arena.MinZ, arena.MaxZ);

            return new Position3D(x, position.Y, z);
        }
    }
}
