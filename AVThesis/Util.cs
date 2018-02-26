using System;

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
            //TODO why not return 0 (or int.MAX) in the case of no visits to avoid the use of 'double.Epsilon'?
            // perhaps add a small random value to avoid super greedy behaviour, two percent points
            return (score / (visits + double.Epsilon)) + 2 * C * Math.Sqrt(Math.Log(parentVisits) / (visits + double.Epsilon));
        }
        
    }
}
