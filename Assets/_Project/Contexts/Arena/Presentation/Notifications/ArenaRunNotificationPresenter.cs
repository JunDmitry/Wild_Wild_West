using System;
using System.Collections.Generic;
using Game.Arena.Application.Notifications;
using Game.Arena.Application.Ports;
using Game.Arena.Application.ReadModels;
using Game.Arena.Domain.Identity;
using Game.Arena.Presentation.Accessibility;
using Game.Arena.Presentation.Combat;
using Game.Arena.Presentation.Windows;
using UnityEngine;

namespace Game.Arena.Presentation.Notifications
{
    public sealed class ArenaRunNotificationPresenter : MonoBehaviour, IArenaRunNotificationSink
    {
        [SerializeField] private HitConfirmationPresenter _hitConfirmation;
        [SerializeField] private ArenaRunAnnouncer _announcer;
        [SerializeField] private VictoryView _victoryView;

        private readonly List<IArenaRunNotification> _pending = new();

        private ArenaRunSnapshot _snapshot;

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

        public int PendingCount => _pending.Count;

        public void Present(ArenaRunSnapshot snapshot)
        {
            _snapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));
            Drain();
        }

        public void Publish(IArenaRunNotification notification)
        {
            if (notification == null)
            {
                throw new ArgumentNullException(nameof(notification));
            }

            _pending.Add(notification);
            Drain();
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

        private void Drain()
        {
            ArenaRunSnapshot snapshot = _snapshot;

            if (snapshot == null || _pending.Count == 0)
            {
                return;
            }

            IArenaRunNotification[] pending = _pending.ToArray();
            _pending.Clear();

            for (int index = 0; index < pending.Length; index++)
            {
                if (pending[index] is ArenaRunEnemyDamagedNotification damaged
                    && damaged.ArenaRunId == snapshot.ArenaRunId)
                {
                    PresentDamage(snapshot, damaged);
                }
                else if (pending[index] is ArenaRunEnemyAttackResolvedNotification resolved
                    && resolved.ArenaRunId == snapshot.ArenaRunId)
                {
                    PresentAttackOutcome(resolved);
                }
                else if (pending[index] is ArenaRunVictoryNotification victory
                    && victory.ArenaRunId == snapshot.ArenaRunId)
                {
                    PresentVictory(victory);
                }
            }
        }

        private void PresentDamage(
            ArenaRunSnapshot snapshot,
            ArenaRunEnemyDamagedNotification notification)
        {
            EnemySnapshot enemy = FindEnemy(snapshot, notification.EnemyId);

            if (enemy != null)
            {
                RequireHitConfirmation().Show(notification.EnemyId, enemy.Position);
            }

            RequireAnnouncer().AnnounceHitOnce(notification.ArenaRunId, notification.Revision);
        }

        private void PresentAttackOutcome(ArenaRunEnemyAttackResolvedNotification notification)
        {
            if (notification.Outcome == ArenaRunEnemyAttackOutcome.Hit)
            {
                return;
            }

            RequireAnnouncer().AnnounceEnemyAttackMissedOnce(
                notification.ArenaRunId,
                notification.Revision,
                notification.EnemyId);
        }

        private void PresentVictory(ArenaRunVictoryNotification notification)
        {
            RequireVictoryView().Show(notification.ArenaRunId);
            RequireAnnouncer().AnnounceVictoryOnce(
                notification.ArenaRunId,
                notification.Revision);
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
