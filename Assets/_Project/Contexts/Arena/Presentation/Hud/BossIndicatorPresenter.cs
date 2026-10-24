using System;
using Game.Arena.Application.Hud;
using Game.Arena.Application.ReadModels;
using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Identity;
using Game.Arena.Presentation.Accessibility;
using UnityEngine;

namespace Game.Arena.Presentation.Hud
{
    public sealed class BossIndicatorPresenter : MonoBehaviour
    {
        public const string RegionLabel = "Boss health";

        [SerializeField] private HudView _hudView;
        [SerializeField] private ArenaRunAnnouncer _announcer;

        private ArenaRunId _presentedRunId;
        private bool _wasBossPresent;
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

        public void Present(ArenaRunSnapshot snapshot, ArenaRunHudSnapshot hud)
        {
            if (snapshot == null)
            {
                throw new ArgumentNullException(nameof(snapshot));
            }

            if (hud == null)
            {
                throw new ArgumentNullException(nameof(hud));
            }

            HudView view = RequireHudView();

            if (hud.IsBossPresent)
            {
                view.ShowBoss(
                    hud.BossLabel,
                    hud.BossHealthText,
                    hud.BossHealthCurrent,
                    hud.BossHealthMaximum,
                    hud.BossHealthFraction);
            }
            else
            {
                view.HideBoss();
            }

            bool isNewRun = _hasPresented == false || snapshot.ArenaRunId != _presentedRunId;
            bool enters = hud.IsBossPresent && isNewRun == false && _wasBossPresent == false;
            bool isDefeated = hud.IsBossPresent == false && isNewRun == false && _wasBossPresent;

            _hasPresented = true;
            _presentedRunId = snapshot.ArenaRunId;
            _wasBossPresent = hud.IsBossPresent;

            if (snapshot.Status != ArenaRunStatus.Playing)
            {
                return;
            }

            if (enters)
            {
                RequireAnnouncer().AnnounceBossEnters();
            }
            else if (isDefeated)
            {
                RequireAnnouncer().AnnounceBossDefeated();
            }
        }

        private HudView RequireHudView()
        {
            HudView view = HudView;

            if (view == null)
            {
                throw new InvalidOperationException("BossIndicatorPresenter requires a HudView.");
            }

            return view;
        }

        private ArenaRunAnnouncer RequireAnnouncer()
        {
            ArenaRunAnnouncer announcer = Announcer;

            if (announcer == null)
            {
                throw new InvalidOperationException("BossIndicatorPresenter requires an ArenaRunAnnouncer.");
            }

            return announcer;
        }
    }
}
