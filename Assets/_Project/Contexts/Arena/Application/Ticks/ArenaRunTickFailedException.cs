using System;

namespace Game.Arena.Application.Ticks
{
    public sealed class ArenaRunTickFailedException : Exception
    {
        public ArenaRunTickFailedException(
            ArenaRunTickResult partialResult,
            Exception inner)
            : base("The gameplay step failed after executing part of its stages.", inner)
        {
            if (partialResult == null)
            {
                throw new ArgumentNullException(nameof(partialResult));
            }

            if (inner == null)
            {
                throw new ArgumentNullException(nameof(inner));
            }

            PartialResult = partialResult;
        }

        public ArenaRunTickResult PartialResult { get; }
    }
}
