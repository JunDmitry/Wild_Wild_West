using System;
using System.Collections.Generic;
using Game.Arena.Domain.Geometry;
using Game.Arena.Domain.Identity;
using UnityEngine;

namespace Game.Arena.Presentation.Combat
{
    public sealed class HitConfirmationPresenter : MonoBehaviour
    {
        [SerializeField] private HitConfirmationView _template;
        [SerializeField] private Transform _root;
        [SerializeField] private float _lifetimeSeconds = 0.25f;

        private readonly Dictionary<EnemyId, HitConfirmationView> _active = new();
        private readonly Stack<HitConfirmationView> _pool = new();
        private readonly List<EnemyId> _expired = new();

        public float LifetimeSeconds
        {
            get => _lifetimeSeconds;
            set => _lifetimeSeconds = value;
        }

        public int ActiveCount => _active.Count;

        public IReadOnlyCollection<EnemyId> ActiveEnemies => _active.Keys;

        public bool IsShownFor(EnemyId enemyId)
        {
            return _active.ContainsKey(enemyId);
        }

        public Position3D PositionOf(EnemyId enemyId)
        {
            return _active[enemyId].Position;
        }

        public void Show(EnemyId enemyId, Position3D position)
        {
            if (enemyId.IsNone)
            {
                throw new ArgumentException("EnemyId cannot be None.", nameof(enemyId));
            }

            if (_active.TryGetValue(enemyId, out HitConfirmationView existing))
            {
                existing.Show(position);
                return;
            }

            HitConfirmationView created = TakeView(enemyId);
            created.LifetimeSeconds = _lifetimeSeconds;
            created.Show(position);

            _active.Add(enemyId, created);
        }

        public void Advance(float deltaSeconds)
        {
            _expired.Clear();

            foreach (KeyValuePair<EnemyId, HitConfirmationView> entry in _active)
            {
                if (entry.Value.Advance(deltaSeconds))
                {
                    _expired.Add(entry.Key);
                }
            }

            for (int index = 0; index < _expired.Count; index++)
            {
                EnemyId enemyId = _expired[index];
                HitConfirmationView view = _active[enemyId];
                _active.Remove(enemyId);
                Release(view);
            }
        }

        public void HideAll()
        {
            foreach (HitConfirmationView view in _active.Values)
            {
                Release(view);
            }

            _active.Clear();
        }

        private void Update()
        {
            Advance(Time.deltaTime);
        }

        private HitConfirmationView TakeView(EnemyId enemyId)
        {
            if (_pool.Count > 0)
            {
                return _pool.Pop();
            }

            HitConfirmationView created = CreateView();
            created.name = "HitConfirmation " + enemyId;

            return created;
        }

        private void Release(HitConfirmationView view)
        {
            view.Hide();
            _pool.Push(view);
        }

        private HitConfirmationView CreateView()
        {
            if (_template != null)
            {
                return Instantiate(_template, Root);
            }

            GameObject created = new GameObject("HitConfirmation");
            created.transform.SetParent(Root, false);

            return created.AddComponent<HitConfirmationView>();
        }

        private Transform Root => _root != null ? _root : transform;
    }
}
