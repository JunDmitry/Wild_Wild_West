using Game.Core.Model.Configs;
using Game.Core.Model.Entities;
using Game.Core.Model.States;
using Game.Core.Rules.Mathematics;

namespace Game.Core.Rules
{
    public static class MovementRules
    {
        private const float Epsilon = .0001f;

        public static PlayerState MovePlayer(
            in PlayerState player,
            in FrameInput input,
            in FrameContext context,
            in ArenaConfig arena,
            float moveSpeed,
            float delta)
        {
            Direction3D direction = Math3D.NormalizeXZ(input.MoveDirection);
            float length = Math3D.LengthXZ(input.MoveDirection);

            if (length <= Epsilon || context.MovementBlocked)
            {
                return player;
            }

            float scale = Math3D.Clamp01(length);
            float distanceDelta = moveSpeed * scale * delta;
            Position3D candidate = Math3D.OffsetXZ(player.Position, direction, distanceDelta);

            candidate = ClampToArena(candidate, arena);

            return new PlayerState(
                player.Id,
                candidate,
                player.CurrentHealth,
                player.MaxHealth,
                player.SelectedWeapon,
                player.RangedReadyTime,
                player.MeleeReadyTime);
        }

        public static Position3D ClampToArena(Position3D position, ArenaConfig arena)
        {
            float x = Math3D.Clamp(position.X, arena.MinX, arena.MaxX);
            float z = Math3D.Clamp(position.Z, arena.MinZ, arena.MaxZ);

            return new Position3D(x, position.Y, z);
        }
    }
}
