namespace Game.Arena.Domain.Geometry
{
    internal static class GeometryTolerance
    {
        public const float DirectionUnitLength = 0.0001f;

        public const float MinimumDirectionLength = 0.0001f;

        public const float MovementInputMaximumAcceptedLength = 1.0001f;

        public const float BoundsTolerance = 0.0001f;

        public const float MovementPathTolerance = 0.001f;

        public const float CombatRangeTolerance = 0.001f;
    }
}
