using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Copyright © 2019 Anton Valkenberg
/// Written by BSc. A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by MSc. G.-J. Roelofs, MSc. T. Aliyev and MSc. D. de Rydt.
/// </summary>
namespace AVThesis.Datastructures {

    public static class Extensions {

        private static readonly Random _r = new Random();

        /// <summary>
        /// Returns a random element from this enumeration.
        /// </summary>
        /// <typeparam name="T">The Type of this enumeration.</typeparam>
        /// <param name="collection">This enumeration.</param>
        /// <returns>A random element or the default value of T if the enumeration is empty.</returns>
        public static T RandomElementOrDefault<T>(this IEnumerable<T> collection) {
            var enumerable = collection.ToList();
            // If there are no elements in the list, return the default value of T
            return !enumerable.Any() ? default(T) : enumerable.ElementAt(_r.Next(enumerable.Count));
        }

        /// <summary>
        /// Determines if this enumeration is null or empty.
        /// </summary>
        /// <typeparam name="T">The Type of this enumeration.</typeparam>
        /// <param name="list">This enumeration.</param>
        /// <returns>Whether or not this enumeration is null or empty.</returns>
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> list) {
            return list == null || !list.Any();
        }

        /// <summary>
        /// Shuffles this enumeration. This is done by assigning a random number to each item and returning a new enumerable ordered by these numbers.
        /// </summary>
        /// <typeparam name="T">The Type of this enumeration.</typeparam>
        /// <param name="list">This enumeration.</param>
        /// <returns>A new, shuffled version of this enumeration.</returns>
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> list) {
            var r = new Random((int)DateTime.Now.Ticks);
            return list.Select(x => new { Number = r.Next(), Item = x }).OrderBy(x => x.Number).Select(x => x.Item);
        }

        /// <summary>
        /// Checks the value of a struct to see if it equals the default value.
        /// </summary>
        /// <typeparam name="T">The Type of this struct.</typeparam>
        /// <param name="value">The value of this struct.</param>
        /// <returns>Whether or not this struct's value is equals to the default value.</returns>
        public static bool IsDefault<T>(this T value) where T : struct {
           return value.Equals(default(T));
        }

    }

}
