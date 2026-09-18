namespace Game.Arena.Domain.Movement
{
    public enum MovementPathVerdict
    {
        Accepted = 0,
        OffGroundPlane = 1,
        BehindOrigin = 2,
        BeyondRequestedDistance = 3,
        OffMovementPath = 4,
    }
}
