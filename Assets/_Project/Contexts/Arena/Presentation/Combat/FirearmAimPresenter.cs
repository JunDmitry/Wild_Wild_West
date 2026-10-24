using System;
using Game.Arena.Application.ReadModels;
using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Combat;
using Game.Arena.Presentation.Accessibility;
using UnityEngine;

namespace Game.Arena.Presentation.Combat
{
    public sealed class FirearmAimPresenter : MonoBehaviour
    {
        [SerializeField] private FirearmAimMarkerView _markerView;
        [SerializeField] private ViewDirectionTracker _viewDirection;
        [SerializeField] private ArenaRunAnnouncer _announcer;

        private ArenaRunSnapshot _snapshot;
        private bool _wasVisible;

        public FirearmAimMarkerView MarkerView
        {
            get => _markerView != null ? _markerView : GetComponentInChildren<FirearmAimMarkerView>();
            set => _markerView = value;
        }

        public ViewDirectionTracker ViewDirection
        {
            get => _viewDirection != null ? _viewDirection : GetComponentInChildren<ViewDirectionTracker>();
            set => _viewDirection = value;
        }

        public ArenaRunAnnouncer Announcer
        {
            get => _announcer != null ? _announcer : GetComponentInChildren<ArenaRunAnnouncer>();
            set => _announcer = value;
        }

        public bool IsAimVisible => MarkerView != null && MarkerView.IsVisible;

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
                && snapshot.Player.SelectedWeapon == WeaponKind.Ranged;

            if (isVisible)
            {
                FirearmAimMarkerView marker = MarkerView;

                if (marker == null)
                {
                    throw new InvalidOperationException("FirearmAimPresenter requires a FirearmAimMarkerView.");
                }

                marker.Show(snapshot.Player.Position, RequireViewDirection().Current);
            }
            else
            {
                MarkerView?.Hide();
            }

            if (isVisible && _wasVisible == false)
            {
                RequireAnnouncer().AnnounceAimDirection();
            }

            _wasVisible = isVisible;
        }

        private void Update()
        {
            Refresh();
        }

        private ViewDirectionTracker RequireViewDirection()
        {
            ViewDirectionTracker tracker = ViewDirection;

            if (tracker == null)
            {
                throw new InvalidOperationException("FirearmAimPresenter requires a ViewDirectionTracker.");
            }

            return tracker;
        }

        private ArenaRunAnnouncer RequireAnnouncer()
        {
            ArenaRunAnnouncer announcer = Announcer;

            if (announcer == null)
            {
                throw new InvalidOperationException("FirearmAimPresenter requires an ArenaRunAnnouncer.");
            }

            return announcer;
        }
    }
}
