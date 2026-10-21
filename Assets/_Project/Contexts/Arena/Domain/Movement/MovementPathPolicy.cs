using System;
using Game.Arena.Domain.Geometry;

namespace Game.Arena.Domain.Movement
{
    public sealed class MovementPathPolicy
    {
        public MovementPathVerdict Validate(PlanarMovementIntent intent, Position3D acceptedPosition)
        {
            if (intent.IsValid == false)
            {
                throw new ArgumentException("Movement intent is invalid.", nameof(intent));
            }

            float tolerance = GeometryTolerance.MovementPathTolerance;

            if (Math.Abs(acceptedPosition.Y - intent.From.Y) > tolerance)
            {
                return MovementPathVerdict.OffGroundPlane;
            }

            Displacement3D travel = acceptedPosition - intent.From;
            float along = travel.Dot(intent.Direction);

            if (along < -tolerance)
            {
                return MovementPathVerdict.BehindOrigin;
            }

            if (along > intent.RequestedDistance.Value + tolerance)
            {
                return MovementPathVerdict.BeyondRequestedDistance;
            }

            float lateralSquared = travel.LengthSquared - (along * along);

            if (lateralSquared > tolerance * tolerance)
            {
                return MovementPathVerdict.OffMovementPath;
            }

            return MovementPathVerdict.Accepted;
        }
    }
}
