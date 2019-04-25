using System;
using System.Collections.Generic;

/// <summary>
/// Copyright © 2019 Anton Valkenberg
/// Written by BSc. A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by MSc. G.-J. Roelofs, MSc. T. Aliyev and MSc. D. de Rydt.
/// </summary>
namespace AVThesis.Datastructures {

    // ReSharper disable once CommentTypo
    /// <summary>
    /// An implementation of the alias method implemented using Vose's algorithm.
    /// The alias method allows for efficient sampling of random values from a discrete probability distribution (i.e. rolling a loaded die).
    /// Process runs in O(1) time after O(n) pre-processing time.
    /// Source: https://www.gamasutra.com/view/news/168153/Simulating_a_loaded_dice_in_a_constant_time.php
    /// Source: http://www.keithschwarz.com/darts-dice-coins/
    /// </summary>
    /// <typeparam name="T">The Type of the items in the table.</typeparam>
    public class OddmentTable<T> {

        #region Fields

        /// <summary>
        /// The alias table used in the `AliasMethod'.
        /// </summary>
        private int[] _alias;

        /// <summary>
        /// The probability table used in the `AliasMethod'.
        /// </summary>
        private double[] _probability;

        /// <summary>
        /// The collection of options contained in this table.
        /// </summary>
        protected List<Tuple<double, T>> Options = new List<Tuple<double, T>>();

        /// <summary>
        /// RandomNumberGenerator (RNG).
        /// </summary>
        protected Random Random;

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs a new OddmentTable with a preset RNG.
        /// </summary>
        /// <param name="random">The RNG to use.</param>
        /// <param name="options">The options that should be contained in the table.</param>
        public OddmentTable(Random random, List<Tuple<double, T>> options) {
            Random = random;
            Recalculate(options);
        }

        /// <summary>
        /// Constructs a new OddmentTable with a preset RNG.
        /// </summary>
        /// <param name="random">The RNG to use.</param>
        public OddmentTable(Random random) {
            Random = random;
        }

        /// <summary>
        /// Constructs a new OddmentTable where the RNG is seeded with the current TickCount.
        /// </summary>
        public OddmentTable() {
            Random = new Random(Environment.TickCount);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Recalculates the table using the internal Options.
        /// </summary>
        public void Recalculate() {
            Recalculate(Options);
        }

        /// <summary>
        /// Recalculates the tables using the provided Options.
        /// </summary>
        /// <param name="allOptions">The new options to use for this OddmentTable.</param>
        public void Recalculate(List<Tuple<double, T>> allOptions) {

            // Save the options and get a summation that we'll need for normalising.
            Options = allOptions;
            double sum = 0;
            foreach (var option in Options) {
                sum += option.Item1;
            }

            _alias = new int[Options.Count];
            _probability = new double[Options.Count];
            var average = 1.0 / Options.Count;

            // Normalise the probabilities.
            var newProbabilities = new List<double>();
            foreach (var option in Options) {
                newProbabilities.Add(Util.Normalise(option.Item1, 0, sum));
            }

            // Create two stacks to act as work-lists as we populate the tables.
            var small = new Deque<int>();
            var large = new Deque<int>();

            // Populate the stacks with the input probabilities' indices.
            for (var i = 0; i < newProbabilities.Count; i++) {
                if (newProbabilities[i] >= average) {
                    large.Add(i);
                }
                else {
                    small.Add(i);
                }
            }

            // As a note: in the mathematical specification of the algorithm, we will always exhaust the small list before the big list.
            // However, due to floating point inaccuracies, this is not necessarily true.
            // Consequently, this inner loop (which tries to pair small and large elements) will have to check that both lists aren't empty.
            while (!small.IsEmpty && !large.IsEmpty) {
                var less = small.RemoveBack();
                var more = large.RemoveBack();

                // These probabilities have not yet been scaled up to be such that 1/n is given weight 1.0.
                // We do this here instead.
                _probability[less] = newProbabilities[less] * newProbabilities.Count;
                _alias[less] = more;

                // Decrease the probability of the larger one by the appropriate amount.
                newProbabilities[more] = newProbabilities[more] + newProbabilities[less] - average;

                // If the new probability is less than the average, add it into the small list; otherwise add it to the large list.
                if (newProbabilities[more] >= 1.0 / newProbabilities.Count) {
                    large.Add(more);
                }
                else {
                    small.Add(more);
                }
            }

            // At this point, everything is in one list, which means that the remaining probabilities should all be 1/n.
            // Based on this, set them appropriately.
            // Due to numerical issues, we can't be sure which stack will hold the entries, so we empty both.
            while (!small.IsEmpty) _probability[small.RemoveBack()] = 1.0;
            while (!large.IsEmpty) _probability[large.RemoveBack()] = 1.0;
        }

        /// <summary>
        /// Samples a value from the underlying distribution.
        /// </summary>
        /// <returns>A random value based on this OddmentTable's distribution.</returns>
        public T Next() {
            // Generate a fair die roll to determine which column to inspect.
            var column = Random.Next(_probability.Length);

            // Generate a biased coin toss to determine which option to pick in this column.
            var coinToss = Random.NextDouble() < _probability[column];

            // Based on the outcome, return either the column or its alias.
            return coinToss ? Options[column].Item2 : Options[_alias[column]].Item2;
        }

        /// <summary>
        /// Adds an option to this OddmentTable.
        /// Recalculation of the table is optional.
        /// </summary>
        /// <param name="option">The option to add.</param>
        /// <param name="recalculate">[Optional] Whether or not to recalculate the table. Default value is true.</param>
        public void Add(Tuple<double, T> option, bool recalculate = true) {
            Options.Add(option);
            if (recalculate) Recalculate();
        }

        /// <summary>
        /// Adds a value to the OddmentTable with a specified chance of selection.
        /// Recalculation of the table is optional.
        /// </summary>
        /// <param name="value">The value to add.</param>
        /// <param name="chance">The chance of selection for the value.</param>
        /// <param name="recalculate">[Optional] Whether or not to recalculate the table. Default value is true.</param>
        public void Add(T value, double chance, bool recalculate = true) {
            Options.Add(new Tuple<double, T>(chance, value));
            if (recalculate) Recalculate();
        }

        /// <summary>
        /// Adds a collection of options to this OddmentTable and recalculates the table.
        /// </summary>
        /// <param name="collection">The collection to add.</param>
        public void AddAll(List<Tuple<double, T>> collection) {
            Options.AddRange(collection);
            Recalculate();
        }

        /// <summary>
        /// Whether or not this OddmentTable contains the specified value.
        /// </summary>
        /// <param name="value">The value to check.</param>
        /// <returns>Whether or not this OddmentTable contains the specified value.</returns>
        public bool Contains(Tuple<double, T> value) {
            return Options.Contains(value);
        }

        /// <summary>
        /// Removes the specified value from this OddmentTable and recalculates the table.
        /// </summary>
        /// <param name="value">The value to remove.</param>
        /// <returns>Whether or not the value was successfully removed.</returns>
        public bool RemoveValue(Tuple<double, T> value) {
            var success = Options.Remove(value);
            Recalculate();
            return success;
        }

        /// <summary>
        /// Removes the option stored at the given index in this OddmentTable and recalculates the table.
        /// </summary>
        /// <param name="index">The index to remove.</param>
        public void RemoveAt(int index) {
            Options.RemoveAt(index);
            Recalculate();
        }

        /// <summary>
        /// Removes all options from this OddmentTable.
        /// </summary>
        public void Clear() {
            Options.Clear();
            Recalculate();
        }

        #endregion

    }

}
