using System;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Identity;
using UnityEngine;

namespace Game.Arena.Presentation.Accessibility
{
    public sealed class ArenaRunAnnouncer : MonoBehaviour
    {
        public const string AimDirectionAnnouncement = "Aim direction";
        public const string HitAnnouncement = "Hit";

        private bool _hasAnnouncedHit;
        private ArenaRunId _announcedRunId;
        private AggregateRevision _announcedRevision;

        public event Action<string> Announced;

        public int AnnouncementCount { get; private set; }

        public string LastAnnouncement { get; private set; }

        public void AnnounceAimDirection()
        {
            Announce(AimDirectionAnnouncement);
        }

        public void AnnounceHitOnce(ArenaRunId arenaRunId, AggregateRevision revision)
        {
            if (arenaRunId.IsNone)
            {
                throw new ArgumentException("ArenaRunId cannot be None.", nameof(arenaRunId));
            }

            if (_hasAnnouncedHit
                && _announcedRunId == arenaRunId
                && _announcedRevision == revision)
            {
                return;
            }

            _hasAnnouncedHit = true;
            _announcedRunId = arenaRunId;
            _announcedRevision = revision;

            Announce(HitAnnouncement);
        }

        private void Announce(string announcement)
        {
            AnnouncementCount++;
            LastAnnouncement = announcement;
            Announced?.Invoke(announcement);
        }
    }
}
