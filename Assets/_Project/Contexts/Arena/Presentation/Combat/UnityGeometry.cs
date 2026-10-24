using Game.Arena.Domain.Geometry;
using UnityEngine;

namespace Game.Arena.Presentation.Combat
{
    internal static class UnityGeometry
    {
        public static Vector3 ToVector3(Position3D position)
        {
            return new Vector3(position.X, position.Y, position.Z);
        }

        public static Vector3 ToVector3(Direction3D direction)
        {
            return new Vector3(direction.X, direction.Y, direction.Z);
        }
    }
}
