namespace Analyzers.Core.Extensions
{
    using System;
    using System.Collections.Generic;

    public static class EnumerableExtensions
    {
        public static IEnumerable<T> Do<T>(this IEnumerable<T> source, Action<T> action)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (action == null)
            {
                throw new ArgumentNullException(nameof(action));
            }

            var list = source as IList<T>;

            if (list != null)
            {
                for (int i = 0, count = list.Count; i < count; i++)
                {
                    action(list[i]);
                }
            }
            else
            {
                foreach (var value in source)
                {
                    action(value);
                }
            }

            return source;
        }

        public static bool IsSorted<T>(this IEnumerable<T> enumerable, IComparer<T> comparer)
        {
            using (var e = enumerable.GetEnumerator())
            {
                if (!e.MoveNext())
                {
                    return true;
                }

                var previous = e.Current;

                while (e.MoveNext())
                {
                    if (comparer.Compare(previous, e.Current) > 0)
                    {
                        return false;
                    }

                    previous = e.Current;
                }

                return true;
            }
        }
    }
}