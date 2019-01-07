using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Written by A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by G.J. Roelofs and T. Aliyev.
/// </summary>
namespace AVThesis.Datastructures {

    public static class Extentions {

        private static Random _r = new Random();

        /// <summary>
        /// Returns a random element from this enumeration.
        /// </summary>
        /// <typeparam name="T">The Type of this enumeration.</typeparam>
        /// <param name="collection">This enumeration.</param>
        /// <returns>A random element or the default value of T if the enumeration is empty.</returns>
        public static T RandomElementOrDefault<T>(this IEnumerable<T> list) {
            // If there are no elements in the list, return the default value of T
            if (list.Count() == 0) {
                return default(T);
            }

            return list.ElementAt(_r.Next(list.Count()));
        }

        /// <summary>
        /// Determines if this enumeration is null or empty.
        /// </summary>
        /// <typeparam name="T">The Type of this enumeration.</typeparam>
        /// <param name="list">This enumeration.</param>
        /// <returns>Whether or not this enumeration is null or empty.</returns>
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> list) {
            return list == null || list.Count() == 0;
        }

        /// <summary>
        /// Shuffles this enumeration. This is done by assiging a random number to each item and returning a new enumerable ordered by these numbers.
        /// </summary>
        /// <typeparam name="T">The Type of this enumeration.</typeparam>
        /// <param name="list">This enumeration.</param>
        /// <returns>A new, shuffled version of this enumeration.</returns>
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> list) {
            var r = new Random((int)DateTime.Now.Ticks);
            return list.Select(x => new { Number = r.Next(), Item = x }).OrderBy(x => x.Number).Select(x => x.Item);
        }

    }

}
