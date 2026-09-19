using Game.Arena.Application.Ports;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Interactions.Movement;

namespace Game.Arena.Application.Tests.Ticks
{
    internal sealed class ScriptedPlayerMovementResolver : IPlayerMovementResolver
    {
        public enum ResolveMode
        {
            AcceptRequested = 0,
            StayAtOrigin = 1,
            ReturnFixed = 2,
        }

        public ResolveMode Mode { get; set; }

        public Position3D FixedPosition { get; set; }

        public int CallCount { get; private set; }

        public PlayerMovementRequest LastRequest { get; private set; }

        public Position3D Resolve(PlayerMovementRequest request)
        {
            CallCount++;
            LastRequest = request;

            if (Mode == ResolveMode.StayAtOrigin)
            {
                return request.Intent.From;
            }

            if (Mode == ResolveMode.ReturnFixed)
            {
                return FixedPosition;
            }

            return request.Intent.RequestedPosition;
        }
    }
}
