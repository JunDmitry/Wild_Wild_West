using Assets.Scripts.Gameplay.Common.Interfaces;
using System.Collections.Generic;

namespace Assets.Scripts.Gameplay.Effect_Feature.Interfaces
{
    public interface IEffectProducer : IModel
    {
        int ProducerId { get; }
        IReadOnlyList<IEffectDescriptor> Effects { get; }
    }
}