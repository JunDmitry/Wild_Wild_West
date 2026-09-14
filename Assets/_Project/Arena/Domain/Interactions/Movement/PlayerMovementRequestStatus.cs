namespace Game.Arena.Domain.Interactions.Movement
{
    public enum PlayerMovementRequestStatus
    {
        Requested = 0,
        RunIsNotPlaying = 1,
        NoMovement = 2,
        PositionUnchanged = 3,
    }
}
