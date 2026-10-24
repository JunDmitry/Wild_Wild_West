using System;
using Game.Arena.Application.Hud;
using Game.Arena.Application.ReadModels;
using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Combat;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Identity;
using UnityEngine;

namespace Game.Arena.Presentation.Combat
{
    public sealed class BossEntryMarkerPresenter : MonoBehaviour
    {
        [SerializeField] private BossEntryMarkerView _template;
        [SerializeField] private Transform _root;

        private ArenaRunSnapshot _snapshot;
        private ArenaRunHudSnapshot _hud;
        private BossEntryMarkerView _view;
        private ArenaRunId _presentedRunId;
        private bool _wasBossPresent;
        private bool _hasPresented;

        public BossEntryMarkerView Template
        {
            get => _template;
            set => _template = value;
        }

        public bool IsVisible => _view != null && _view.IsVisible;

        public string Label => BossEntryMarkerView.StatusLabel;

        public Position3D Position => _view == null ? Position3D.Zero : _view.Position;

        public void Present(ArenaRunSnapshot snapshot, ArenaRunHudSnapshot hud)
        {
            _snapshot = snapshot ?? throw new ArgumentNullException(nameof(snapshot));
            _hud = hud ?? throw new ArgumentNullException(nameof(hud));

            Refresh();
        }

        public void Refresh()
        {
            ArenaRunSnapshot snapshot = _snapshot;

            if (snapshot == null || _hud == null)
            {
                return;
            }

            bool isNewRun = _hasPresented == false || snapshot.ArenaRunId != _presentedRunId;
            bool enters = _hud.IsBossPresent && isNewRun == false && _wasBossPresent == false;

            _hasPresented = true;
            _presentedRunId = snapshot.ArenaRunId;
            _wasBossPresent = _hud.IsBossPresent;

            if (enters
                && snapshot.Status == ArenaRunStatus.Playing
                && TryFindPresentedBoss(snapshot, out EnemySnapshot boss))
            {
                Show(boss.Position);
                return;
            }

            Hide();
        }

        private static bool TryFindPresentedBoss(
            ArenaRunSnapshot snapshot,
            out EnemySnapshot boss)
        {
            for (int index = 0; index < snapshot.Enemies.Count; index++)
            {
                EnemySnapshot enemy = snapshot.Enemies[index];

                if (enemy.Kind != EnemyKind.Boss)
                {
                    continue;
                }

                if (snapshot.Bounds.Contains(enemy.Position, enemy.CollisionRadius) == false)
                {
                    continue;
                }

                boss = enemy;
                return true;
            }

            boss = null;
            return false;
        }

        private void Show(Position3D position)
        {
            if (_view == null)
            {
                _view = CreateView();
                _view.name = "BossEntryMarker";
            }

            _view.Show(position);
        }

        private void Hide()
        {
            if (_view == null)
            {
                return;
            }

            _view.Hide();
        }

        private BossEntryMarkerView CreateView()
        {
            if (_template != null)
            {
                return Instantiate(_template, Root);
            }

            GameObject created = new GameObject("BossEntryMarker");
            created.transform.SetParent(Root, false);
            created.AddComponent<LineRenderer>();

            return created.AddComponent<BossEntryMarkerView>();
        }

        private Transform Root => _root != null ? _root : transform;
    }
}
