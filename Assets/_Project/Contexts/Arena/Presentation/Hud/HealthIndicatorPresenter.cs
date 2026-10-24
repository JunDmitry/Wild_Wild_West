using System;
using Game.Arena.Application.Hud;
using Game.Arena.Presentation.Accessibility;
using UnityEngine;

namespace Game.Arena.Presentation.Hud
{
    public sealed class HealthIndicatorPresenter : MonoBehaviour
    {
        [SerializeField] private HudView _hudView;
        [SerializeField] private ArenaRunAnnouncer _announcer;

        private bool _hasPresented;

        public HudView HudView
        {
            get => _hudView != null ? _hudView : GetComponentInChildren<HudView>();
            set => _hudView = value;
        }

        public ArenaRunAnnouncer Announcer
        {
            get => _announcer != null ? _announcer : GetComponentInChildren<ArenaRunAnnouncer>();
            set => _announcer = value;
        }

        public void Present(ArenaRunHudSnapshot hud)
        {
            if (hud == null)
            {
                throw new ArgumentNullException(nameof(hud));
            }

            HudView view = RequireHudView();
            bool isChange = _hasPresented == false
                || string.Equals(view.HealthText, hud.HealthText, StringComparison.Ordinal) == false;

            view.ShowHealth(
                hud.HealthLabel,
                hud.HealthText,
                hud.HealthCurrent,
                hud.HealthMaximum,
                hud.HealthFraction,
                hud.IsDefeated);

            if (isChange)
            {
                RequireAnnouncer().AnnounceHealth(hud.HealthLabel, hud.HealthText, hud.IsDefeated);
            }

            _hasPresented = true;
        }

        private HudView RequireHudView()
        {
            HudView view = HudView;

            if (view == null)
            {
                throw new InvalidOperationException("HealthIndicatorPresenter requires a HudView.");
            }

            return view;
        }

        private ArenaRunAnnouncer RequireAnnouncer()
        {
            ArenaRunAnnouncer announcer = Announcer;

            if (announcer == null)
            {
                throw new InvalidOperationException("HealthIndicatorPresenter requires an ArenaRunAnnouncer.");
            }

            return announcer;
        }
    }
}
