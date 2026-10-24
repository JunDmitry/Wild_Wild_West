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
            _wasPending.UnionWith(_pending);
        }

        private void Update()
        {
            Refresh();
        }

        private void ShowCue(EnemySnapshot enemy)
        {
            if (_active.TryGetValue(enemy.EnemyId, out EnemyAttackCueView existing))
            {
                existing.Show(enemy.Position);
                return;
            }

            EnemyAttackCueView created = CreateView();
            created.name = "EnemyAttackCue " + enemy.EnemyId;
            created.Show(enemy.Position);

            _active.Add(enemy.EnemyId, created);
        }

        private void RemoveResolvedCues()
        {
            List<EnemyId> resolved = null;

            foreach (KeyValuePair<EnemyId, EnemyAttackCueView> entry in _active)
            {
                if (_pending.Contains(entry.Key))
                {
                    continue;
                }

                resolved ??= new List<EnemyId>();
                resolved.Add(entry.Key);
            }

            if (resolved == null)
            {
                return;
            }

            for (int index = 0; index < resolved.Count; index++)
            {
                Destroy(_active[resolved[index]].gameObject);
                _active.Remove(resolved[index]);
            }
        }

        private void HideAll()
        {
            foreach (EnemyAttackCueView view in _active.Values)
            {
                Destroy(view.gameObject);
            }

            _active.Clear();
            _pending.Clear();
            _wasPending.Clear();
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
