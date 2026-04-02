using Assets.Scripts.Gameplay.Common.Interfaces;
using System;
using System.Collections.Generic;

namespace Assets.Scripts.Gameplay.Stat_Feature
{
    public interface IStatsModel : IModel
    {
        event Action<IStat> StatChanged;

        IReadOnlyList<Stat> Stats { get; }
    }
}