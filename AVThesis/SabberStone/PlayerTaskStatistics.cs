﻿using System.Collections.Generic;

/// <summary>
/// Copyright © 2019 Anton Valkenberg
/// Written by BSc. A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by MSc. G.-J. Roelofs, MSc. T. Aliyev and MSc. D. de Rydt.
/// </summary>
namespace AVThesis.SabberStone {

    /// <summary>
    /// A class used to track some statistics for a SabberStonePlayerTask.
    /// </summary>
    public class PlayerTaskStatistics {

        #region Properties

        /// <summary>
        /// The SabberStonePlayerTask that these statistics concern.
        /// </summary>
        public SabberStonePlayerTask Task { get; }

        /// <summary>
        /// The total amount of value that has been recorded for this SabberStonePlayerTask.
        /// </summary>
        public double TotalValue { get; set; }

        /// <summary>
        /// The number of times this SabberStonePlayerTask has been visited.
        /// </summary>
        public int Visits { get; private set; }

        /// <summary>
        /// The collection of values.
        /// </summary>
        public List<double> ValueCollection { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of PlayerTaskStatistics.
        /// </summary>
        /// <param name="task">The SabberStonePlayerTask that these statistics concern.</param>
        /// <param name="value">The initial value for this task.</param>
        public PlayerTaskStatistics(SabberStonePlayerTask task, double value) {
            Task = task;
            Visits = 0;
            TotalValue = 0;
            ValueCollection = new List<double>();

            AddValue(value);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds a value to this task and increments the visit count.
        /// </summary>
        /// <param name="value">The value to be added to this task.</param>
        public void AddValue(double value) {
            ValueCollection.Add(value);
            TotalValue += value;
            Visits++;
        }

        /// <summary>
        /// Calculates the average value for this task.
        /// </summary>
        /// <returns>The total value for this task divided by the amount of visits.</returns>
        public double AverageValue() => TotalValue / Visits;

        /// <summary>
        /// Returns the UCB value of this task.
        /// </summary>
        /// <param name="parentVisits">The amount of visits to the task's parent.</param>
        /// <param name="c">The c-parameter to be used.</param>
        /// <returns>UCB value of this task, see <see cref="Util.UCB"/>.</returns>
        public double UCB(int parentVisits, double c) {
            return Util.UCB(TotalValue, Visits, parentVisits, c);
        }

        public override string ToString() {
            return $"{Task} | total value: {TotalValue}, visits: {Visits}";
        }

        #endregion

    }
}
