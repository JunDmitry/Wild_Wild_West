using System;

namespace Assets.Scripts.Gameplay.PlayerFeature.Components
{
    public interface IResolver
    {
        T Instantiate<T>(params object[] extraArgs);

        T Instantiate<T>();

        object Instantiate(Type type);

        object Instantiate(Type type, params object[] extraArgs);

        T Resolve<T>();

        object Resolve(Type type);
    }
}