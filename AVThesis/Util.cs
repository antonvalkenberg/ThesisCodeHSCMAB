using System;
using System.Collections.Generic;
using AVThesis.Datastructures;
using SabberStoneCore.Enums;
using SabberStoneCore.Model;

/// <summary>
/// Copyright © 2019 Anton Valkenberg
/// Written by BSc. A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by MSc. G.-J. Roelofs, MSc. T. Aliyev and MSc. D. de Rydt.
/// </summary>
namespace AVThesis {

    /// <summary>
    /// Utility class for various arithmetic (or otherwise useful) methods.
    /// </summary>
    public static class Util {

        /// <summary>
        /// A random number generator.
        /// </summary>
        public static Random RNG = new Random();

        /// <summary>
        /// Calculates UCB.
        /// </summary>
        /// <param name="score">The total score acquired by the node.</param>
        /// <param name="visits">The amount of visits to the node.</param>
        /// <param name="parentVisits">The amount of visits to the parent of the node.</param>
        /// <param name="c">A constant. Should be tuned experimentally.</param>
        /// <returns>Double representing the UCB value.</returns>
        public static double UCB(double score, int visits, int parentVisits, double c) {
            //TODO UCB: perhaps add a small random value to avoid super greedy behaviour, two percent points
            return score / (visits + double.Epsilon) + 2 * c * Math.Sqrt(Math.Log(parentVisits) / (visits + double.Epsilon));
        }

        /// <summary>
        /// Represents a HiddenCard; an opponent's card of which the identity is not known.
        /// </summary>
        public static Card HiddenCard => new Card {
            AssetId = -1,
            Id = "HD_001",
            Name = "Hidden Card",
            Text = "Shh, it's hidden.",
            Entourage = new string[0],
            Tags = new Dictionary<GameTag, int> {
                { GameTag.CARDTYPE, (int)CardType.SPELL }
            },
            RefTags = new Dictionary<GameTag, int>(),
            PlayRequirements = new Dictionary<PlayReq, int>()
        };

        // ReSharper disable once CommentTypo
        /// <summary>
        /// Required for the Deque class.
        /// Source: https://github.com/tejacques/Deque/blob/master/src/Deque/Utility.cs
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static int ClosestPowerOfTwoGreaterThan(int x) {
            x--;
            x |= x >> 1;
            x |= x >> 2;
            x |= x >> 4;
            x |= x >> 8;
            x |= x >> 16;
            return x + 1;
        }

        /// <summary>
        /// Normalises a value in the range of [min .. max] to the range of [0.0 .. 1.0].
        /// </summary>
        /// <param name="x">The value to be normalised.</param>
        /// <param name="min">The minimum value.</param>
        /// <param name="max">The maximum value.</param>
        /// <returns>Normalised value of x.</returns>
        public static double Normalise(double x, double min, double max) {
            return (x - min) / (max - min);
        }

        /// <summary>
        /// Calculates the Shannon entropy of a collection of double values.
        /// See <see cref="http://mathworld.wolfram.com/Entropy.html"/>
        /// </summary>
        /// <param name="values">The values to calculate the entropy over.</param>
        /// <returns></returns>
        public static double ShannonEntropy(ICollection<double> values) {
            if (values.IsNullOrEmpty()) return 0;

            // The (Shannon) entropy of a variable X is defined as:
            // H(X) = -1 * Sum-over-x(P(x) * Log_b(P(x)))
            // where b is the base of the logarithm (2 is said to be a commonly used value)
            
            // Create a table of counts for the values
            var table = new Dictionary<double, int>();
            foreach (var value in values) {
                if (!table.ContainsKey(value)) table.Add(value, 0);
                table[value]++;
            }

            var runningSummation = 0d;
            foreach (var tableKey in table.Keys) {
                // Get the chance of the value occurring
                var pX = table[tableKey] / (values.Count * 1.0);
                runningSummation += pX * Math.Log(pX, 2);
            }

            return -1 * runningSummation;
        }

    }
}
