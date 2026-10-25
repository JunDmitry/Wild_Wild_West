using System;
using System.Collections.Generic;
using Game.Arena.Application.Dispatch;
using Game.Arena.Application.Ticks;

namespace Game.Arena.Application.Tests.Dispatch
{
    internal sealed class RecordingDispatch : IArenaRunDispatch
    {
        private readonly List<ArenaRunTickResult> _dispatched = new();

        public IReadOnlyList<ArenaRunTickResult> Dispatched => _dispatched;

        public Exception FailWith { get; set; }

        public void Dispatch(ArenaRunTickResult result)
        {
            if (FailWith != null)
            {
                throw FailWith;
            }

            _dispatched.Add(result);
        }
    }
}
