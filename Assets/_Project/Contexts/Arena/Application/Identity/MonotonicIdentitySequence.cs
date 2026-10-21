using System;

namespace Game.Arena.Application.Identity
{
    internal sealed class MonotonicIdentitySequence
    {
        private readonly object _sync;

        private ulong _nextValue;
        private bool _exhausted;

        public MonotonicIdentitySequence(ulong firstValue)
        {
            if (firstValue == 0UL)
            {
                throw new ArgumentOutOfRangeException(nameof(firstValue));
            }

            _sync = new object();
            _nextValue = firstValue;
        }

        public ulong Allocate()
        {
            lock (_sync)
            {
                if (_exhausted)
                {
                    throw new InvalidOperationException("Identity sequence has been exhausted.");
                }

                ulong allocatedValue = _nextValue;

                if (allocatedValue == ulong.MaxValue)
                {
                    _exhausted = true;
                }
                else
                {
                    _nextValue = allocatedValue + 1UL;
                }

                return allocatedValue;
            }
        }
    }
}
