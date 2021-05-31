using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace MyLinq
{
    static class Enumerable
    {
        public static IEnumerable<TResult> Empty<TResult>()
        {
            return new List<TResult>();
        }

        public static IEnumerable<TResult> Repeat<TResult>(TResult element, int count)
        {
            if (count < 0)
                throw new ArgumentOutOfRangeException();

            for (var i = 0; count > i; i++)
                yield return element;
        }

        public static IEnumerable<int> Range(int start, int count)
        {
            if (count < 0 || start + count - 1 > int.MaxValue)
                throw new ArgumentOutOfRangeException();

            for (var i = 0; count > i; i++)
                yield return start + i;
        }

        public static IEnumerable<TSource> AsEnumerable<TSource>(this IEnumerable<TSource> source)
        {
            foreach (var num in source)
                yield return num;
        }

        public static IEnumerable<TResult> Cast<TResult>(this IEnumerable source)
        {
            if (source == null)
                throw new ArgumentNullException();

            foreach (var num in source)
            {
                if (num is not TResult)
                {
                    throw new InvalidCastException();
                }

                yield return (TResult) num;
            }
        }

        public static TSource[] ToArray<TSource>(this IEnumerable<TSource> source)
        {
            if (source == null)
                throw new ArgumentNullException();

            var result = new TSource[source.Count()];
            var enumerator = source.GetEnumerator();
            for (var index = 0; enumerator.MoveNext(); index++)
                result[index] = enumerator.Current;
            
            return result;
        }

        public static List<TSource> ToList<TSource>(this IEnumerable<TSource> source)
        {
            if (source == null)
                throw new ArgumentNullException();

            var result = new List<TSource>();
            foreach (var num in source)
                result.Add(num);

            return result;
        }

        public static Dictionary<TKey, TElement> ToDictionary<TSource, TKey, TElement>(this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector)
        {
            return source.ToDictionary(keySelector, elementSelector, null);
        }

        public static Dictionary<TKey, TElement> ToDictionary<TSource, TKey, TElement>(this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector, Func<TSource, TElement> elementSelector, IEqualityComparer<TKey>? comparer)
        {
            if (source == null || keySelector == null || elementSelector == null)
                throw new ArgumentNullException();

            comparer ??= EqualityComparer<TKey>.Default;
            var result = new Dictionary<TKey, TElement>(comparer);
            foreach (var num in source)
            {
                var key = keySelector(num);
                var element = elementSelector(num);
                if (result.ContainsKey(key))
                    throw new ArgumentException();
                result.Add(key, element);
            }

            return result;
        }

        public static Dictionary<TKey, TSource> ToDictionary<TSource, TKey>(this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector)
        {
            return source.ToDictionary(keySelector, null);
        }

        public static Dictionary<TKey, TSource> ToDictionary<TSource, TKey>(this IEnumerable<TSource> source,
            Func<TSource, TKey> keySelector, IEqualityComparer<TKey>? comparer)
        {
            return source.ToDictionary(keySelector, x => x, comparer);
        }

        public static bool Contains<TSource>(this IEnumerable<TSource> source, TSource value)
        {
            return source.Contains(value, null);
        }

        public static bool Contains<TSource>(this IEnumerable<TSource> source, TSource value,
            IEqualityComparer<TSource>? comparer)
        {
            if (source == null)
                throw new ArgumentNullException();
            
            comparer ??= EqualityComparer<TSource>.Default;
            foreach (var num in source)
            {
                if (comparer.Equals(num, value))
                    return true;
            }
            
            return false;
        }

        public static bool All<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            if (source == null || predicate == null)
                throw new ArgumentNullException();

            foreach (var num in source)
            {
                if (!predicate(num))
                    return false;
            }

            return true;
        }

        public static bool Any<TSource>(this IEnumerable<TSource> source)
        {
            if (source == null)
                throw new ArgumentNullException();
            return source.Count() != 0;
        }

        public static bool Any<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            if (source == null)
                throw new ArgumentException();

            var result = false;
            foreach (var num in source)
            {
                result = predicate(num) || result;
            }

            return result;
        }

        public static bool SequenceEqual<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second)
        {
            return first.SequenceEqual(second, null);
        }

        public static bool SequenceEqual<TSource>(this IEnumerable<TSource> first, IEnumerable<TSource> second,
            IEqualityComparer<TSource>? comparer)
        {
            if (first == null || second == null)
                throw new ArgumentNullException();

            comparer ??= EqualityComparer<TSource>.Default;
            
            var firstEnumerator = first.GetEnumerator();
            var secondEnumerator = second.GetEnumerator();

            var result = false;
            while (true)
            {
                var firstNext = firstEnumerator.MoveNext();
                var secondNext = secondEnumerator.MoveNext();
                
                if (firstNext && secondNext)
                {
                    if (!comparer.Equals(firstEnumerator.Current, secondEnumerator.Current))
                        break;
                    continue;
                }
                if (!(firstNext || secondNext))
                    result = true;
                break;
            }

            return result;
        }

        public static TSource ElementAt<TSource>(this IEnumerable<TSource> source, int index)
        {
            if (source == null)
                throw new ArgumentNullException();

            var (result, status) = ElementAtOrDefaultWithStatus(source, index);
            if (!status)
                throw new InvalidOperationException();
            
            return result;
        }

        public static TSource? ElementAtOrDefault<TSource>(this IEnumerable<TSource> source, int index)
        {
            if (source == null)
                throw new ArgumentNullException();

            var (result, _) = ElementAtOrDefaultWithStatus(source, index);
            
            return result;
        }

        private static (TSource?, bool) ElementAtOrDefaultWithStatus<TSource>(IEnumerable<TSource> source, int index)
        {
            var enumerator = source.GetEnumerator();
            for (var i = 0; index >= i; i++)
            {
                var next = enumerator.MoveNext();
                if (!next)
                    return (default, false);
            }

            return (enumerator.Current, true);
        }
        
        public static TSource Single<TSource>(this IEnumerable<TSource> source)
        {
            return source.Single(x => true);
        }

        public static TSource Single<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            if (source == null || predicate == null)
                throw new ArgumentNullException();

            var (result, status) = SingleOrDefaultWithStatus(source, predicate);
            if (!status)
                throw new InvalidOperationException();

            return result;
        }

        public static TSource? SingleOrDefault<TSource>(this IEnumerable<TSource> source)
        {
            return source.SingleOrDefault(x => true);
        }
        
        public static TSource? SingleOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            if (source == null || predicate == null)
                throw new ArgumentNullException();

            var (result, _) = SingleOrDefaultWithStatus(source, predicate);
            
            return result;
        }

        private static (TSource?, bool) SingleOrDefaultWithStatus<TSource>(IEnumerable<TSource> source,
            Func<TSource, bool> predicate)
        {
            var flag = false;
            var result = default(TSource);
            foreach (var num in source)
            {
                if (predicate(num))
                {
                    if (flag)
                        throw new InvalidOperationException();
                    flag = true;
                    result = num;
                }
            }

            if (!flag)
                return (default, false);
            return (result, true);
        }

        public static TSource First<TSource>(this IEnumerable<TSource> source)
        {
            return source.First(x => true);
        }

        public static TSource First<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            if (source == null || predicate == null)
                throw new ArgumentNullException();

            var (result, status) = FirstOrDefaultWithStatus(source, predicate);
            if (!status)
                throw new InvalidOperationException();

            return result;
        }

        public static TSource? FirstOrDefault<TSource>(this IEnumerable<TSource> source)
        {
            return source.FirstOrDefault(x => true);
        }

        public static TSource? FirstOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            if (source == null || predicate == null)
                throw new ArgumentNullException();

            var (result, _) = FirstOrDefaultWithStatus(source, predicate);

            return result;
        }

        private static (TSource?, bool) FirstOrDefaultWithStatus<TSource>(IEnumerable<TSource> source,
            Func<TSource, bool> predicate)
        {
            foreach (var num in source)
            {
                if (predicate(num))
                    return (num, true);
            }

            return (default, false);
        }

        public static TSource Last<TSource>(this IEnumerable<TSource> source)
        {
            return source.Last(x => true);
        }

        public static TSource Last<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            if (source == null || predicate == null)
                throw new ArgumentNullException();

            var (result, status) = LastOrDefaultWithStatus(source, predicate);
            if (!status)
                throw new InvalidOperationException();

            return result;
        }

        public static TSource? LastOrDefault<TSource>(this IEnumerable<TSource> source)
        {
            return source.LastOrDefault(x => true);
        }

        public static TSource? LastOrDefault<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            if (source == null || predicate == null)
                throw new ArgumentNullException();

            var (result, _) = LastOrDefaultWithStatus(source, predicate);
            
            return result;
        }

        private static (TSource?, bool) LastOrDefaultWithStatus<TSource>(IEnumerable<TSource> source,
            Func<TSource, bool> predicate)
        {
            var flag = false;
            var result = default(TSource);
            foreach (var num in source)
            {
                if (predicate(num))
                {
                    flag = true;
                    result = num;
                }
            }

            if (!flag)
                return (default, false);
            return (result, true);
        }
        
        public static int Count<TSource>(this IEnumerable<TSource> source)
        {
            if (source == null)
                throw new ArgumentNullException();
            
            var count = 0;
            foreach (var num in source)
            {
                if (count == int.MaxValue)
                    throw new OverflowException();
                count++;
            }

            return count;
        }

        public static int Count<TSource>(this IEnumerable<TSource> source, Func<TSource, bool> predicate)
        {
            if (source == null || predicate == null)
                throw new ArgumentNullException();
            
            var count = 0;
            foreach (var num in source)
            {
                if (predicate(num))
                {
                    if (count == int.MaxValue)
                        throw new OverflowException();
                    count++;
                }
            }

            return count;
        }
    }
}