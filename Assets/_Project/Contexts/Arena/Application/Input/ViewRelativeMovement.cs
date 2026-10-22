using Game.Arena.Domain.Geometry;

namespace Game.Arena.Application.Input
{
    internal static class ViewRelativeMovement
    {
        private const float MinimumMagnitude = 0.0001f;

        public static MovementInput Translate(MovementAxes axes, Direction3D viewDirection)
        {
            if (axes.IsZero)
            {
                return MovementInput.Zero;
            }

            Displacement3D horizontal = new Displacement3D(viewDirection.X, 0f, viewDirection.Z);

            if (horizontal.TryToDirection(out Direction3D forward) == false)
            {
                return MovementInput.Zero;
            }

            Displacement3D right = new Displacement3D(forward.Z, 0f, -forward.X);

            float x = (forward.X * axes.Forward) + (right.X * axes.Right);
            float z = (forward.Z * axes.Forward) + (right.Z * axes.Right);

            Displacement3D translation = new Displacement3D(x, 0f, z);
            float length = translation.Length;

            if (length < MinimumMagnitude)
            {
                return MovementInput.Zero;
            }

            if (length > 1f)
            {
                translation = new Displacement3D(x / length, 0f, z / length);
            }

            return MovementInput.FromVector(translation);
        }
    }
}
