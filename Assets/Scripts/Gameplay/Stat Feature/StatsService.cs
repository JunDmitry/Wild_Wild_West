using Assets.Scripts.Architecture.Repository.Events;
using Assets.Scripts.Architecture.SignalBus.Interfaces;
using Assets.Scripts.Gameplay.Common.Interfaces;
using Assets.Scripts.Gameplay.PlayerFeature.Components;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Assets.Scripts.Gameplay.Stat_Feature
{
    public class StatsService : IDisposable, IStatsService
    {
        private readonly Dictionary<int, IStatsModel> _statsModelById;
        private readonly List<IDisposable> _disposables;

        public StatsService(ISignalBus<IEvent> signalBus)
        {
            _statsModelById = new();
            _disposables = new();

            Subscribe(signalBus);
        }

        public bool TryGetStat(int id, StatType statType, out Stat stat)
        {
            stat = GetStat(id, statType);

            return stat != Stat.Empty;
        }

        public Stat GetStat(int id, StatType statType)
        {
            Stat stat = Stat.Empty;

            if (_statsModelById.TryGetValue(id, out IStatsModel statsModel))
                stat = statsModel.Stats.FirstOrDefault(s => s.Type == statType) ?? stat;

            return stat;
        }

        public bool HasStats(int id, params StatType[] statTypes)
        {
            foreach (StatType statType in statTypes)
                if (HasStat(id, statType) == false)
                    return false;

            return true;
        }

        public bool HasStat(int id, StatType statType)
        {
            Stat stat = GetStat(id, statType);

            return stat != Stat.Empty;
        }

        public void Dispose()
        {
            foreach (IDisposable disposable in _disposables)
                disposable.Dispose();

            _disposables.Clear();
        }

        private void Subscribe(ISignalBus<IEvent> signalBus)
        {
            _disposables.Add(signalBus.Subscribe<ItemAddedIntoRepositoryEvent<IModel>>(OnModelCreated));
            _disposables.Add(signalBus.Subscribe<ItemRemovedFromRepositoryEvent<IModel>>(OnModelRemoved));
        }

        private void OnModelCreated(ItemAddedIntoRepositoryEvent<IModel> modelAddedEvent)
        {
            if (modelAddedEvent.AddedItem is not IStatsModel statsModel)
                return;

            _statsModelById[statsModel.Id] = statsModel;
        }

        private void OnModelRemoved(ItemRemovedFromRepositoryEvent<IModel> @event)
        {
            if (@event.RemovedItem is not IStatsModel statsModel)
                return;

            _statsModelById.Remove(statsModel.Id);
        }
    }
}