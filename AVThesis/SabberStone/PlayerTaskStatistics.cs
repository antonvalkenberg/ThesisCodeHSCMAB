/// <summary>
/// Written by A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by G.J. Roelofs and T. Aliyev.
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
        private double TotalValue { get; set; }

        /// <summary>
        /// The number of times this SabberStonePlayerTask has been visited.
        /// </summary>
        private int Visits { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of PlayerTaskStatistics.
        /// </summary>
        /// <param name="task">The SabberStonePlayerTask that these statistics concern.</param>
        /// <param name="value">The initial value for this task.</param>
        public PlayerTaskStatistics(SabberStonePlayerTask task, double value) {
            Task = task;
            TotalValue = value;
            Visits = 1;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds a value to this task and increments the visit count.
        /// </summary>
        /// <param name="value">The value to be added to this task.</param>
        public void AddValue(double value) {
            TotalValue += value;
            Visits++;
        }

        /// <summary>
        /// Calculates the average value for this task.
        /// </summary>
        /// <returns>The total value for this task divided by the amount of visits.</returns>
        public double AverageValue() => TotalValue / Visits;

        #endregion

    }
}
