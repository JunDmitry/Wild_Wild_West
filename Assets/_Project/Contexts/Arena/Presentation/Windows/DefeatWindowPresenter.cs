using System;
using Game.Arena.Application.Hud;
using Game.Arena.Application.ReadModels;
using Game.Arena.Presentation.Accessibility;
using UnityEngine;

namespace Game.Arena.Presentation.Windows
{
    public sealed class DefeatWindowPresenter : MonoBehaviour
    {
        [SerializeField] private DefeatView _defeatView;
        [SerializeField] private ArenaRunAnnouncer _announcer;

        public DefeatView DefeatView
        {
            get => _defeatView != null ? _defeatView : GetComponentInChildren<DefeatView>();
            set => _defeatView = value;
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

            if (hud.IsDefeated)
            {
                RequireDefeatView().Show(snapshot.ArenaRunId, hud.WaveText);
                RequireAnnouncer().AnnounceDefeatOnce(snapshot.ArenaRunId, hud.WaveText);
                return;
            }

            RequireDefeatView().Hide();
        }

        private DefeatView RequireDefeatView()
        {
            DefeatView view = DefeatView;

            if (view == null)
            {
                throw new InvalidOperationException(
                    "DefeatWindowPresenter requires a DefeatView.");
            }

            return view;
        }

        private ArenaRunAnnouncer RequireAnnouncer()
        {
            ArenaRunAnnouncer announcer = Announcer;

            if (announcer == null)
            {
                throw new InvalidOperationException(
                    "DefeatWindowPresenter requires an ArenaRunAnnouncer.");
            }

            return announcer;
        }
    }
}
