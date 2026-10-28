using System;
using System.Collections.Generic;
using Game.Arena.Application.ReadModels;
using Game.Arena.Domain.Aggregates.ArenaRun;
using Game.Arena.Domain.Identity;
using Game.Arena.Presentation.Accessibility;
using UnityEngine;

namespace Game.Arena.Presentation.Enemies
{
    public sealed class EnemyPresenter : MonoBehaviour
    {
        [SerializeField] private EnemyViewRegistry _registry;
        [SerializeField] private ArenaRunAnnouncer _announcer;

        private readonly HashSet<EnemyId> _wasInside = new();
        private readonly HashSet<EnemyId> _inside = new();

        private ArenaRunId _presentedRunId;
        private bool _hasPresented;

        public EnemyViewRegistry Registry
        {
            get => _registry != null ? _registry : GetComponent<EnemyViewRegistry>();
            set => _registry = value;
        }

        public ArenaRunAnnouncer Announcer
        {
            get => _announcer != null ? _announcer : GetComponentInChildren<ArenaRunAnnouncer>();
            set => _announcer = value;
        }

        public void Present(ArenaRunSnapshot snapshot)
        {
            if (snapshot == null)
            {
                throw new ArgumentNullException(nameof(snapshot));
            }

            RequireRegistry().Sync(snapshot);

            bool isNewRun = _hasPresented == false || snapshot.ArenaRunId != _presentedRunId;

            _hasPresented = true;
            _presentedRunId = snapshot.ArenaRunId;

            HashSet<EnemyId> inside = CollectInside(snapshot);

            if (isNewRun == false && snapshot.Status == ArenaRunStatus.Playing)
            {
                AnnounceEntries(snapshot, inside);
            }

            _wasInside.Clear();
            _wasInside.UnionWith(inside);
        }

        private HashSet<EnemyId> CollectInside(ArenaRunSnapshot snapshot)
        {
            HashSet<EnemyId> inside = _inside;
            inside.Clear();

            for (int index = 0; index < snapshot.Enemies.Count; index++)
            {
                EnemySnapshot enemy = snapshot.Enemies[index];

                if (snapshot.Bounds.Contains(enemy.Position, enemy.CollisionRadius))
                {
                    inside.Add(enemy.EnemyId);
                }
            }

            return inside;
        }

        private void AnnounceEntries(
            ArenaRunSnapshot snapshot,
            HashSet<EnemyId> inside)
        {
            for (int index = 0; index < snapshot.Enemies.Count; index++)
            {
                EnemySnapshot enemy = snapshot.Enemies[index];

                if (inside.Contains(enemy.EnemyId) == false)
                {
                    continue;
                }

                if (_wasInside.Contains(enemy.EnemyId))
                {
                    continue;
                }

                RequireAnnouncer().AnnounceEnemyEnteredOnce(snapshot.ArenaRunId, enemy.EnemyId);
            }
        }

        private EnemyViewRegistry RequireRegistry()
        {
            EnemyViewRegistry registry = Registry;

            if (registry == null)
            {
                throw new InvalidOperationException("EnemyPresenter requires an EnemyViewRegistry.");
            }

            return registry;
        }

        private ArenaRunAnnouncer RequireAnnouncer()
        {
            ArenaRunAnnouncer announcer = Announcer;

            if (announcer == null)
            {
                throw new InvalidOperationException("EnemyPresenter requires an ArenaRunAnnouncer.");
            }

            return announcer;
        }
    }
}
