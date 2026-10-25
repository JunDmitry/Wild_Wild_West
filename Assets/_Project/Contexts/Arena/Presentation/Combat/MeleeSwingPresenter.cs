using System;
using Game.Arena.Application.ReadModels;
using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Combat;
using Game.Arena.Domain.Geometry;
using UnityEngine;

namespace Game.Arena.Presentation.Combat
{
    public sealed class MeleeSwingPresenter : MonoBehaviour
    {
        [SerializeField] private MeleeSwingView _swingView;
        [SerializeField] private ViewDirectionTracker _viewDirection;

        private ArenaRunSnapshot _snapshot;

        public MeleeSwingView SwingView
        {
            get => _swingView != null ? _swingView : GetComponentInChildren<MeleeSwingView>();
            set => _swingView = value;
        }

        public ViewDirectionTracker ViewDirection
        {
            get => _viewDirection != null ? _viewDirection : GetComponentInChildren<ViewDirectionTracker>();
            set => _viewDirection = value;
        }

        public bool IsSwingVisible => SwingView != null && SwingView.IsVisible;

        public Direction3D SwingDirection => SwingView != null ? SwingView.Direction : Direction3D.Forward;

        private void Update()
        {
            Refresh();
        }

        public void Present(ArenaRunSnapshot snapshot)
        {
            _snapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));
            Refresh();
        }

        public void Refresh()
        {
            ArenaRunSnapshot snapshot = _snapshot;

            if (snapshot == null)
            {
                return;
            }

            bool isVisible = snapshot.Status == ArenaRunStatus.Playing
                && snapshot.Player.Health.IsDepleted == false
                && snapshot.Player.HasPendingAttack
                && snapshot.Player.PendingAttack.WeaponKind == WeaponKind.Melee;

            if (isVisible)
            {
                PendingPlayerAttack pendingAttack = snapshot.Player.PendingAttack;

                RequireSwingView().Show(
                    snapshot.Player.Position,
                    RequireViewDirection().Current,
                    pendingAttack.Range,
                    pendingAttack.Arc);
            }
            else
            {
                SwingView?.Hide();
            }
        }

        private MeleeSwingView RequireSwingView()
        {
            MeleeSwingView view = SwingView;

            if (view == null)
            {
                throw new InvalidOperationException("MeleeSwingPresenter requires a MeleeSwingView.");
            }

            return view;
        }

        private ViewDirectionTracker RequireViewDirection()
        {
            ViewDirectionTracker tracker = ViewDirection;

            if (tracker == null)
            {
                throw new InvalidOperationException("MeleeSwingPresenter requires a ViewDirectionTracker.");
            }

            return tracker;
        }
    }
}
