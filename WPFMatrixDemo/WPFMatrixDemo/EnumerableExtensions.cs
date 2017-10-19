using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WPFMatrixDemo
{
    /// <summary>
    /// Extension methods for <see cref="IEnumerable{T}"/>.
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Lazily invokes a given <see cref="Action{T}"/> upon items in an <see cref="IEnumerable{T}"/>.
        /// </summary>
        /// <typeparam name="T">The type of items in the given enumerable.</typeparam>
        /// <param name="this">The enumerable whose items are to be passed to the action</param>
        /// <param name="action">The action to invoke for each item</param>
        /// <returns>A new <see cref="IEnumerable{T}"/> containing instructions for lazily-invoking the action, upon enumeration.</returns>
        public static IEnumerable<T> Do<T>(this IEnumerable<T> @this, Action<T> action)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));

            if (action == null)
                throw new ArgumentNullException(nameof(action));

            return DoInternal(@this, action);
        }

        private static IEnumerable<T> DoInternal<T>(this IEnumerable<T> @this, Action<T> action)
        {
            foreach (var item in @this)
            {
                action.Invoke(item);
                yield return item;
            }
        }

        /// <summary>
        /// Invokes a given action on each item within an <see cref="IEnumerable{T}"/>.
        /// This forces enumeration of the <see cref="IEnumerable{T}"/> and terminates any corresponding Linq query expression.
        /// </summary>
        /// <exception cref="ArgumentNullException">Throws for this and action</exception>
        /// <typeparam name="T">The type of items in the given enumerable.</typeparam>
        /// <param name="this">The enumerable whose items are to be passed to the action</param>
        /// <param name="action">The action to invoke for each item</param>
        public static void ForEach<T>(this IEnumerable<T> @this, Action<T> action)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));

            if (action == null)
                throw new ArgumentNullException(nameof(action));

            foreach (var item in @this)
                action.Invoke(item);
        }

        /// <summary>
        /// Evaluates an <see cref="IEnumerable{T}"/> and terminates any corresponding Linq query expression.
        /// </summary>
        /// <exception cref="ArgumentNullException">Throws for this and action</exception>
        /// <typeparam name="T">The type of items in the given enumerable.</typeparam>
        /// <param name="this">The enumerable to be evaluated</param>
        public static void ForEach<T>(this IEnumerable<T> @this)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));

            var enumerator = @this.GetEnumerator();
            while (enumerator.MoveNext()) ;
        }

        /// <summary>
        /// Wraps a given <see cref="IEnumerable{T}"/> in a new <see cref="IEnumerable{T}"/>.
        /// This prevents consumers from being able to modify the given enumerable by casting it to its underlying type,
        /// without building an entirely new, read-only, collection.
        /// </summary>
        /// <exception cref="ArgumentNullException">Throws for @this.</exception>
        /// <typeparam name="T">The type of items in the given enumerable.</typeparam>
        /// <param name="this">The enumerable to be wrapped.</param>
        /// <returns>A new enumerable, equivalent to the given enumerable.</returns>
        public static IEnumerable<T> AsNewEnumerable<T>(this IEnumerable<T> @this)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));

            // Run yielding in a separate method so that exceptions are thrown when called, not when iterated.
            return AsNewEnumerableInternal(@this);
        }

        private static IEnumerable<T> AsNewEnumerableInternal<T>(IEnumerable<T> @this)
        {
            foreach (var item in @this)
                yield return item;
        }

        /// <summary>
        /// Wraps a single given item in a new <see cref="IEnumerable{T}"/> object, allowing it to be passed around as an enumerable.
        /// </summary>
        /// <typeparam name="T">The type of the given item.</typeparam>
        /// <param name="this">The item to be wrapped as an enumerable.</param>
        /// <returns>A new enumerable, capable of enumerating the single given item.</returns>
        public static IEnumerable<T> MakeEnumerable<T>(this T @this)
        {
            yield return @this;
        }

        /// <summary>
        /// <para>
        /// Computes the Cartesian Product of a given sequence of sequences.
        /// </para>
        /// <para>
        /// The Cartesian Product is every sequence possible sequence containing one item from each of the input sequences, in the same order. 
        /// So, for example, the cartesian product of
        /// <list type="bullet">
        ///     <item>{ A, B }</item>
        ///     <item>{ 1, 2, 3 }</item>
        /// </list>
        /// is
        /// <list type="bullet">
        ///     <item>{ A, 1 }</item>
        ///     <item>{ A, 2 }</item>
        ///     <item>{ A, 3 }</item>
        ///     <item>{ B, 1 }</item>
        ///     <item>{ B, 2 }</item>
        ///     <item>{ B, 3 }</item>
        /// </list>
        /// </para>
        /// <para>
        /// The default product, if no sequences are given, is a sequence containing a single empty sequence.
        /// </para>
        /// </summary>
        /// <typeparam name="T">The type of the items in the input sequences.</typeparam>
        /// <param name="sequences">The input sequences to compute the product of.</param>
        /// <returns>The cartesian product of the input sequences, as described above.</returns>
        public static IEnumerable<IEnumerable<T>> CartesianProduct<T>(this IEnumerable<IEnumerable<T>> sequences)
        {
            if (sequences == null)
                throw new ArgumentNullException(nameof(sequences));

            if (sequences.Any(s => (s == null)))
                throw new ArgumentException("Cannot contain null sequences", nameof(sequences));

            var emptyResult = new[] { Enumerable.Empty<T>() }.AsEnumerable();

            return sequences.Aggregate(
                emptyResult,
                (result, newSequence) => result.SelectMany(resultSequence =>
                                             newSequence.Select(nextSequenceItem => resultSequence.Concat(new[] { nextSequenceItem }))));
        }

        /// <summary>
        /// Splits a given <see cref="IEnumerable{T}"/> into several, sequential, <see cref="IEnumerable{T}"/> chunks,
        /// each containing a given number of items (except for the last item, which may contain fewer).
        /// </summary>
        /// <typeparam name="T">The type of items in the <see cref="IEnumerable{T}"/> to be split.</typeparam>
        /// <param name="this">The <see cref="IEnumerable{T}"/> to be split.</param>
        /// <param name="size">The desired number of items for each chunk of <paramref name="this"/></param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="this"/>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Throws if <paramref name="size"/> is less than 1.</exception>
        /// <returns>An <see cref="IEnumerable{T}"/> containing the split chunks of <paramref name="this"/>.</returns>
        public static IEnumerable<IEnumerable<T>> Partition<T>(this IEnumerable<T> @this, int size)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));

            if (size <= 0)
                throw new ArgumentOutOfRangeException(nameof(size), size, "Must be greater than 0");

            return @this.Select((x, i) => new { Item = x, Index = i })
                        .Partition(x => ((x.Index % size) == (size - 1)))
                        .Select(x => x.Select(y => y.Item));
        }

        /// <summary>
        /// Splits a given <see cref="IEnumerable{T}"/> into several, sequential, <see cref="IEnumerable{T}"/> chunks,
        /// using a given <see cref="Predicate{T}"/> to determine where the source enumeration should be split.
        /// </summary>
        /// <typeparam name="T">The type of items in the <see cref="IEnumerable{T}"/> to be split.</typeparam>
        /// <param name="this">The <see cref="IEnumerable{T}"/> to be split.</param>
        /// <param name="splitSelector">
        /// A <see cref="Predicate{T}"/> which returns true for items from <paramref name="this"/> which should be the last item within the current chunk.
        /// </param>
        /// <exception cref="ArgumentNullException">Throws for <paramref name="this"/> and <paramref name="splitSelector"/>.</exception>
        /// <returns>An <see cref="IEnumerable{T}"/> containing the split chunks of <paramref name="this"/>.</returns>
        public static IEnumerable<IEnumerable<T>> Partition<T>(this IEnumerable<T> @this, Predicate<T> splitSelector)
        {
            if (@this == null)
                throw new ArgumentNullException(nameof(@this));

            if (splitSelector == null)
                throw new ArgumentNullException(nameof(splitSelector));

            return PartitionInternal(@this, splitSelector);
        }

        private static IEnumerable<IEnumerable<T>> PartitionInternal<T>(IEnumerable<T> @this, Predicate<T> splitSelector)
        {
            var chunk = new List<T>();

            foreach (var item in @this)
            {
                chunk.Add(item);
                if (splitSelector.Invoke(item))
                {
                    yield return chunk.ToArray();
                    chunk.Clear();
                }
            }

            if (chunk.Count > 0)
                yield return chunk.ToArray();
        }
    }
}
