using System;
namespace Assets.Scripts.Common.Extensions
{
    public static class FluentExtensions
    {
        public static T With<T>(this T value, Action<T> action)
        {
            action?.Invoke(value);

            return value;
        }

        public static T With<T>(this T value, Action<T> action, Func<T, bool> predicate)
        {
            if (predicate != null && predicate(value))
                action?.Invoke(value);

            return value;
        }
    }
}
