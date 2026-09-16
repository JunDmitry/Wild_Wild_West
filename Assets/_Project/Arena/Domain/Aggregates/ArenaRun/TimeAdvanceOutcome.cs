using System;
using Game.Arena.Domain.Time;

namespace Game.Arena.Domain.Aggregates.ArenaRun
{
    public sealed class TimeAdvanceOutcome
    {
        private TimeAdvanceOutcome(TimeAdvanceStatus status, GameTimePoint currentTime, ArenaRunChange change)
        {
            Status = status;
            CurrentTime = currentTime;
            Change = change ?? throw new ArgumentNullException(nameof(change));
        }

        public TimeAdvanceStatus Status { get; }

        public GameTimePoint CurrentTime { get; }

        public ArenaRunChange Change { get; }

        public bool IsAdvanced => Status == TimeAdvanceStatus.Advanced;

        public static TimeAdvanceOutcome Advanced(GameTimePoint currentTime, ArenaRunChange change)
        {
            return new TimeAdvanceOutcome(TimeAdvanceStatus.Advanced, currentTime, change);
        }

        public static TimeAdvanceOutcome RunIsNotPlaying(GameTimePoint currentTime, ArenaRunChange change)
        {
            return new TimeAdvanceOutcome(TimeAdvanceStatus.RunIsNotPlaying, currentTime, change);
        }

        public static TimeAdvanceOutcome InteractionPending(GameTimePoint currentTime, ArenaRunChange change)
        {
            return new TimeAdvanceOutcome(TimeAdvanceStatus.InteractionPending, currentTime, change);
        }
    }
}
