using System;
using System.Collections.Generic;
using Game.Arena.Application.Dispatch;
using Game.Arena.Application.Notifications;
using Game.Arena.Application.ReadModels;
using Game.Arena.Domain.Identity;
using Game.Arena.Presentation.Accessibility;
using Game.Arena.Presentation.Combat;
using Game.Arena.Presentation.Windows;
using UnityEngine;

namespace Game.Arena.Presentation.Notifications
{
    public sealed class ArenaRunNotificationPresenter : MonoBehaviour,
        IArenaRunSnapshotHandler,
        IArenaRunNotificationHandler<ArenaRunEnemyDamagedNotification>,
        IArenaRunNotificationHandler<ArenaRunEnemyAttackResolvedNotification>,
        IArenaRunNotificationHandler<ArenaRunVictoryNotification>
    {
        [SerializeField] private HitConfirmationPresenter _hitConfirmation;
        [SerializeField] private ArenaRunAnnouncer _announcer;
        [SerializeField] private VictoryView _victoryView;

        private ArenaRunSnapshot _snapshot;
        private ArenaRunSnapshot _previousSnapshot;

        public HitConfirmationPresenter HitConfirmation
        {
            get => _hitConfirmation != null ? _hitConfirmation : GetComponentInChildren<HitConfirmationPresenter>();
            set => _hitConfirmation = value;
        }

        public ArenaRunAnnouncer Announcer
        {
            get => _announcer != null ? _announcer : GetComponentInChildren<ArenaRunAnnouncer>();
            set => _announcer = value;
        }

        public VictoryView VictoryView
        {
            get => _victoryView != null ? _victoryView : GetComponentInChildren<VictoryView>();
            set => _victoryView = value;
        }

        public void Handle(ArenaRunSnapshot snapshot)
        {
            if (snapshot == null)
            {
                throw new ArgumentNullException(nameof(snapshot));
            }

            _previousSnapshot = _snapshot;
            _snapshot = snapshot;
        }

        public void Handle(ArenaRunEnemyDamagedNotification notification)
        {
            if (notification == null)
            {
                throw new ArgumentNullException(nameof(notification));
            }

            if (BelongsToCurrentRun(notification.ArenaRunId) == false)
            {
                return;
            }

            EnemySnapshot enemy = FindEnemy(
                PositionSnapshot(notification.ArenaRunId),
                notification.EnemyId);

            if (enemy != null)
            {
                RequireHitConfirmation().Show(notification.EnemyId, enemy.Position);
            }

            RequireAnnouncer().AnnounceHitOnce(notification.ArenaRunId, notification.Revision);
        }

        public void Handle(ArenaRunEnemyAttackResolvedNotification notification)
        {
            if (notification == null)
            {
                throw new ArgumentNullException(nameof(notification));
            }

            if (BelongsToCurrentRun(notification.ArenaRunId) == false)
            {
                return;
            }

            if (notification.Outcome == ArenaRunEnemyAttackOutcome.Hit)
            {
                return;
            }

            RequireAnnouncer().AnnounceEnemyAttackMissedOnce(
                notification.ArenaRunId,
                notification.Revision,
                notification.EnemyId);
        }

        public void Handle(ArenaRunVictoryNotification notification)
        {
            if (notification == null)
            {
                throw new ArgumentNullException(nameof(notification));
            }

            if (BelongsToCurrentRun(notification.ArenaRunId) == false)
            {
                return;
            }

            RequireVictoryView().Show(notification.ArenaRunId);
            RequireAnnouncer().AnnounceVictoryOnce(
                notification.ArenaRunId,
                notification.Revision);
        }

        private bool BelongsToCurrentRun(ArenaRunId arenaRunId)
        {
            return _snapshot != null && _snapshot.ArenaRunId == arenaRunId;
        }

        private ArenaRunSnapshot PositionSnapshot(ArenaRunId arenaRunId)
        {
            if (_previousSnapshot != null && _previousSnapshot.ArenaRunId == arenaRunId)
            {
                return _previousSnapshot;
            }

            return _snapshot;
        }

        private static EnemySnapshot FindEnemy(ArenaRunSnapshot snapshot, EnemyId enemyId)
        {
            IReadOnlyList<EnemySnapshot> enemies = snapshot.Enemies;

            for (int index = 0; index < enemies.Count; index++)
            {
                if (enemies[index].EnemyId == enemyId)
                {
                    return enemies[index];
                }
            }

            return null;
        }

        private HitConfirmationPresenter RequireHitConfirmation()
        {
            HitConfirmationPresenter presenter = HitConfirmation;

            if (presenter == null)
            {
                throw new InvalidOperationException(
                    "ArenaRunNotificationPresenter requires a HitConfirmationPresenter.");
            }

            return presenter;
        }

        private VictoryView RequireVictoryView()
        {
            VictoryView view = VictoryView;

            if (view == null)
            {
                throw new InvalidOperationException(
                    "ArenaRunNotificationPresenter requires a VictoryView.");
            }

            return view;
        }

        private ArenaRunAnnouncer RequireAnnouncer()
        {
            ArenaRunAnnouncer announcer = Announcer;

            if (announcer == null)
            {
                throw new InvalidOperationException(
                    "ArenaRunNotificationPresenter requires an ArenaRunAnnouncer.");
            }

            return announcer;
        }
    }
}
