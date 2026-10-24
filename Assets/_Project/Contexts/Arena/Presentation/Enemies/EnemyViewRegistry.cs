using System;
using System.Collections.Generic;
using Game.Arena.Application.ReadModels;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Identity;
using UnityEngine;

namespace Game.Arena.Presentation.Enemies
{
    public sealed class EnemyViewRegistry : MonoBehaviour
    {
        [SerializeField] private EnemyView _template;
        [SerializeField] private Transform _root;

        private readonly Dictionary<EnemyId, EnemyView> _views = new();
        private readonly HashSet<EnemyId> _presented = new();

        private ArenaRunId _presentedRunId;
        private bool _hasPresented;

        public EnemyView Template
        {
            get => _template;
            set => _template = value;
        }

        public int Count => _views.Count;

        public bool IsRendered(EnemyId enemyId)
        {
            return _views.ContainsKey(enemyId);
        }

        public bool TryGetView(EnemyId enemyId, out EnemyView view)
        {
            return _views.TryGetValue(enemyId, out view);
        }

        public Position3D PositionOf(EnemyId enemyId)
        {
            return _views[enemyId].Position;
        }

        public void Sync(ArenaRunSnapshot snapshot)
        {
            if (snapshot == null)
            {
                throw new ArgumentNullException(nameof(snapshot));
            }

            if (_hasPresented == false || snapshot.ArenaRunId != _presentedRunId)
            {
                ClearAll();
            }

            _hasPresented = true;
            _presentedRunId = snapshot.ArenaRunId;

            _presented.Clear();

            for (int index = 0; index < snapshot.Enemies.Count; index++)
            {
                EnemySnapshot enemy = snapshot.Enemies[index];

                if (snapshot.Bounds.Contains(enemy.Position, enemy.CollisionRadius) == false)
                {
                    continue;
                }

                _presented.Add(enemy.EnemyId);
                Show(enemy);
            }

            RemoveUnpresented();
        }

        private void Show(EnemySnapshot enemy)
        {
            if (_views.TryGetValue(enemy.EnemyId, out EnemyView existing))
            {
                existing.Show(enemy.Position);
                return;
            }

            EnemyView view = CreateView();
            view.name = "Enemy " + enemy.EnemyId;
            view.Show(enemy.Position);

            _views.Add(enemy.EnemyId, view);
        }

        private void RemoveUnpresented()
        {
            List<EnemyId> removed = null;

            foreach (KeyValuePair<EnemyId, EnemyView> entry in _views)
            {
                if (_presented.Contains(entry.Key))
                {
                    continue;
                }

                removed ??= new List<EnemyId>();
                removed.Add(entry.Key);
            }

            if (removed == null)
            {
                return;
            }

            for (int index = 0; index < removed.Count; index++)
            {
                Destroy(_views[removed[index]].gameObject);
                _views.Remove(removed[index]);
            }
        }

        private void ClearAll()
        {
            foreach (EnemyView view in _views.Values)
            {
                Destroy(view.gameObject);
            }

            _views.Clear();
            _presented.Clear();
        }

        private EnemyView CreateView()
        {
            if (_template != null)
            {
                return Instantiate(_template, Root);
            }

            GameObject created = new GameObject("Enemy");
            created.transform.SetParent(Root, false);

            return created.AddComponent<EnemyView>();
        }

        private Transform Root => _root != null ? _root : transform;
    }
}
