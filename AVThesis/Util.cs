using System;
using System.Collections.Generic;
using SabberStoneCore.Enums;
using SabberStoneCore.Model;
using SabberStoneCore.Tasks;

/// <summary>
/// Written by A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by G.J. Roelofs and T. Aliyev.
/// </summary>
namespace AVThesis {

    /// <summary>
    /// Utility class for various arithmetic (or otherwise useful) methods.
    /// </summary>
    public static class Util {

        /// <summary>
        /// Calculates UCB.
        /// </summary>
        /// <param name="score">The total score aquired by the node.</param>
        /// <param name="visits">The amount of visits to the node.</param>
        /// <param name="parentVisits">The amount of visits to the parent of the node.</param>
        /// <param name="C">A constant. Should be tuned experimentally.</param>
        /// <returns>Double representing the UCB value.</returns>
        public static double UCB(double score, int visits, int parentVisits, double C) {
            //TODO for UCB: why not return 0 (or int.MAX) in the case of no visits to avoid the use of 'double.Epsilon'?
            // perhaps add a small random value to avoid super greedy behaviour, two percent points
            return (score / (visits + double.Epsilon)) + 2 * C * Math.Sqrt(Math.Log(parentVisits) / (visits + double.Epsilon));
        }

        /// <summary>
        /// Represents a HiddenCard; an opponent's card of which the identity is not known.
        /// </summary>
        public static Card HiddenCard => new Card() {
            AssetId = -1,
            Id = "HD_001",
            Name = "Hidden Card",
            Text = "Shhh, it's hidden.",
            Entourage = new string[0],
            Tags = new Dictionary<GameTag, int>() {
                { GameTag.CARDTYPE, (int)CardType.SPELL }
            },
            RefTags = new Dictionary<GameTag, int>(),
            PlayRequirements = new Dictionary<PlayReq, int>()
        };

        /// <summary>
        /// Required for the Deque class.
        /// Source: https://github.com/tejacques/Deque/blob/master/src/Deque/Utility.cs
        /// </summary>
        /// <param name="x"></param>
        /// <returns></returns>
        public static int ClosestPowerOfTwoGreaterThan(int x) {
            x--;
            x |= (x >> 1);
            x |= (x >> 2);
            x |= (x >> 4);
            x |= (x >> 8);
            x |= (x >> 16);
            return (x + 1);
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

    }
}
