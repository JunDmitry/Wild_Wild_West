using System;
using Game.Arena.Domain.Geometry;

namespace Game.Arena.Domain.Configuration
{
    public sealed class ArenaDefinition
    {
        public ArenaDefinition(ArenaBounds bounds, Distance spawnBand)
        {
            if (spawnBand.IsZero)
            {
                throw new ArgumentOutOfRangeException(nameof(spawnBand));
            }

            Bounds = bounds;
            SpawnBand = spawnBand;
        }

        public ArenaBounds Bounds { get; }
        public Distance SpawnBand { get; }

        public bool IsSpawnPosition(Position3D position)
        {
            if (position.Y != Bounds.GroundY)
            {
                return false;
            }

            bool insideArena =
                position.X >= Bounds.MinimumX
                && position.X <= Bounds.MaximumX
                && position.Z >= Bounds.MinimumZ
                && position.Z <= Bounds.MaximumZ;

            if (insideArena)
            {
                return false;
            }

            float band = SpawnBand.Value;

            return position.X >= Bounds.MinimumX - band
                && position.X <= Bounds.MaximumX + band
                && position.Z >= Bounds.MinimumZ - band
                && position.Z <= Bounds.MaximumZ + band;
        }
    }
}
