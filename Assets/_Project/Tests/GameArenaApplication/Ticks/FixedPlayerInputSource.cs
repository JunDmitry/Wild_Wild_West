using System;
using Game.Arena.Application.Input;
using Game.Arena.Application.Ports;

namespace Game.Arena.Application.Tests.Ticks
{
    internal sealed class FixedPlayerInputSource : IPlayerInputSource
    {
        public PlayerFrameInput Input { get; set; }

        public PlayerFrameInput Read()
        {
            if (Input == null)
            {
                throw new InvalidOperationException("Input has not been configured.");
            }

            return Input;
        }
    }
}
