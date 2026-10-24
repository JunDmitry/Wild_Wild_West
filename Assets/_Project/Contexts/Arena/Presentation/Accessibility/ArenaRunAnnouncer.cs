using System;
using System.Collections.Generic;
using Game.Arena.Domain.Concurrency;
using Game.Arena.Domain.Identity;
using UnityEngine;

namespace Game.Arena.Presentation.Accessibility
{
    public sealed class ArenaRunAnnouncer : MonoBehaviour
    {
        public const string AimDirectionAnnouncement = "Aim direction";
        public const string HitAnnouncement = "Hit";
        public const string EnemyAttackIncomingAnnouncement = "Enemy attack incoming";
        public const string EnemyAttackMissedAnnouncement = "Enemy attack missed";
        public const string PlayerDefeatedSuffix = "; the player can no longer act";

        private readonly HashSet<(ArenaRunId ArenaRunId, AggregateRevision Revision, EnemyId EnemyId)>
            _announcedEnemyAttackMisses = new();

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

        public void AnnounceEquippedWeapon(string label, string text)
        {
            if (string.IsNullOrWhiteSpace(label))
            {
                throw new ArgumentException("Equipped weapon label cannot be empty.", nameof(label));
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentException("Equipped weapon text cannot be empty.", nameof(text));
            }

            Announce(label + ": " + text);
        }

        public void AnnounceHealth(string label, string text, bool isDefeated)
        {
            if (string.IsNullOrWhiteSpace(label))
            {
                throw new ArgumentException("Health label cannot be empty.", nameof(label));
            }

            if (string.IsNullOrWhiteSpace(text))
            {
                throw new ArgumentException("Health text cannot be empty.", nameof(text));
            }

            string announcement = label + ": " + text;

            if (isDefeated)
            {
                announcement += PlayerDefeatedSuffix;
            }

            Announce(announcement);
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

        public void AnnounceEnemyAttackIncoming()
        {
            Announce(EnemyAttackIncomingAnnouncement);
        }

        public void AnnounceEnemyAttackMissedOnce(
            ArenaRunId arenaRunId,
            AggregateRevision revision,
            EnemyId enemyId)
        {
            if (arenaRunId.IsNone)
            {
                throw new ArgumentException("ArenaRunId cannot be None.", nameof(arenaRunId));
            }

            if (enemyId.IsNone)
            {
                throw new ArgumentException("EnemyId cannot be None.", nameof(enemyId));
            }

            if (_announcedEnemyAttackMisses.Add((arenaRunId, revision, enemyId)) == false)
            {
                return;
            }

            Announce(EnemyAttackMissedAnnouncement);
        }

        private void Announce(string announcement)
        {
            AnnouncementCount++;
            LastAnnouncement = announcement;
            Announced?.Invoke(announcement);
        }
    }
}
