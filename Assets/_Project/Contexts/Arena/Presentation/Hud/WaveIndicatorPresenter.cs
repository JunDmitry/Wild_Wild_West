using System;
using Game.Arena.Application.Hud;
using Game.Arena.Application.ReadModels;
using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Identity;
using Game.Arena.Presentation.Accessibility;
using UnityEngine;

namespace Game.Arena.Presentation.Hud
{
    public sealed class WaveIndicatorPresenter : MonoBehaviour
    {
        public const string WaveLabel = "Wave";

        [SerializeField] private HudView _hudView;
        [SerializeField] private ArenaRunAnnouncer _announcer;

        private ArenaRunId _presentedRunId;
        private int _presentedWave;
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

            RequireHudView().ShowWave(WaveLabel, hud.WaveText);

            bool isNewRun = _hasPresented == false || snapshot.ArenaRunId != _presentedRunId;
            bool beginsWave = isNewRun || hud.WaveCurrent != _presentedWave;

            _hasPresented = true;
            _presentedRunId = snapshot.ArenaRunId;
            _presentedWave = hud.WaveCurrent;

            if (beginsWave == false || snapshot.Status != ArenaRunStatus.Playing)
            {
                return;
            }

            RequireAnnouncer().AnnounceWave(hud.WaveText);
        }

        private HudView RequireHudView()
        {
            HudView view = HudView;

            if (view == null)
            {
                throw new InvalidOperationException("WaveIndicatorPresenter requires a HudView.");
            }

            return view;
        }

        private ArenaRunAnnouncer RequireAnnouncer()
        {
            ArenaRunAnnouncer announcer = Announcer;

            if (announcer == null)
            {
                throw new InvalidOperationException("WaveIndicatorPresenter requires an ArenaRunAnnouncer.");
            }

            return announcer;
        }
    }
}
