using System;
using System.Collections.Generic;

namespace Assets.Scripts.Common.Extensions
{
    public static class EnumerableExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> values, Action<T> action)
        {
            if (values == null || action == null)
                return;

            foreach (T item in values)
                action(item);
        }
    }
}
