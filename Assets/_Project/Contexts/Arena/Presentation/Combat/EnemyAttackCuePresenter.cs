using System;
using System.Collections.Generic;
using Game.Arena.Application.ReadModels;
using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Identity;
using Game.Arena.Presentation.Accessibility;
using UnityEngine;

namespace Game.Arena.Presentation.Combat
{
    public sealed class EnemyAttackCuePresenter : MonoBehaviour
    {
        [SerializeField] private EnemyAttackCueView _template;
        [SerializeField] private Transform _root;
        [SerializeField] private ArenaRunAnnouncer _announcer;

        private readonly Dictionary<EnemyId, EnemyAttackCueView> _active = new();
        private readonly HashSet<EnemyId> _pending = new();
        private readonly HashSet<EnemyId> _wasPending = new();
        private readonly Stack<EnemyAttackCueView> _pool = new();
        private readonly List<EnemyId> _resolved = new();

        private ArenaRunSnapshot _snapshot;

        public EnemyAttackCueView Template
        {
            get => _template;
            set => _template = value;
        }

        public ArenaRunAnnouncer Announcer
        {
            get => _announcer != null ? _announcer : GetComponentInChildren<ArenaRunAnnouncer>();
            set => _announcer = value;
        }

        public int ActiveCount => _active.Count;

        private void Update()
        {
            Refresh();
        }

        public bool IsShownFor(EnemyId enemyId)
        {
            return _active.ContainsKey(enemyId);
        }

        public Position3D PositionOf(EnemyId enemyId)
        {
            return _active[enemyId].Position;
        }

        public string LabelOf(EnemyId enemyId)
        {
            return _active[enemyId].Label;
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

            if (snapshot.Status != ArenaRunStatus.Playing || snapshot.Player.Health.IsDepleted)
            {
                HideAll();
                return;
            }

            _pending.Clear();

            for (int index = 0; index < snapshot.Enemies.Count; index++)
            {
                EnemySnapshot enemy = snapshot.Enemies[index];

                if (enemy.HasPendingAttack == false)
                {
                    continue;
                }

                _pending.Add(enemy.EnemyId);

                if (_wasPending.Contains(enemy.EnemyId) == false)
                {
                    RequireAnnouncer().AnnounceEnemyAttackIncoming();
                }

                ShowCue(enemy);
            }

            RemoveResolvedCues();

            _wasPending.Clear();

            foreach (EnemyId enemyId in _pending)
            {
                _wasPending.Add(enemyId);
            }
        }

        private void ShowCue(EnemySnapshot enemy)
        {
            if (_active.TryGetValue(enemy.EnemyId, out EnemyAttackCueView existing))
            {
                existing.Show(enemy.Position);
                return;
            }

            EnemyAttackCueView created = TakeView(enemy.EnemyId);
            created.Show(enemy.Position);

            _active.Add(enemy.EnemyId, created);
        }

        private void RemoveResolvedCues()
        {
            _resolved.Clear();

            foreach (KeyValuePair<EnemyId, EnemyAttackCueView> entry in _active)
            {
                if (_pending.Contains(entry.Key) == false)
                {
                    _resolved.Add(entry.Key);
                }
            }

            for (int index = 0; index < _resolved.Count; index++)
            {
                EnemyId enemyId = _resolved[index];
                EnemyAttackCueView view = _active[enemyId];
                _active.Remove(enemyId);
                Release(view);
            }
        }

        private void HideAll()
        {
            foreach (EnemyAttackCueView view in _active.Values)
            {
                Release(view);
            }

            _active.Clear();
            _pending.Clear();
            _wasPending.Clear();
        }

        private EnemyAttackCueView TakeView(EnemyId enemyId)
        {
            if (_pool.Count > 0)
            {
                return _pool.Pop();
            }

            EnemyAttackCueView created = CreateView();
            created.name = "EnemyAttackCue " + enemyId;

            return created;
        }

        private void Release(EnemyAttackCueView view)
        {
            view.Hide();
            _pool.Push(view);
        }

        private EnemyAttackCueView CreateView()
        {
            if (_template != null)
            {
                return Instantiate(_template, Root);
            }

            GameObject created = new GameObject("EnemyAttackCue");
            created.transform.SetParent(Root, false);
            created.AddComponent<LineRenderer>();

            return created.AddComponent<EnemyAttackCueView>();
        }

        private Transform Root => _root != null ? _root : transform;

        private ArenaRunAnnouncer RequireAnnouncer()
        {
            ArenaRunAnnouncer announcer = Announcer;

            if (announcer == null)
            {
                throw new InvalidOperationException(
                    "EnemyAttackCuePresenter requires an ArenaRunAnnouncer.");
            }

            return announcer;
        }
    }
}
