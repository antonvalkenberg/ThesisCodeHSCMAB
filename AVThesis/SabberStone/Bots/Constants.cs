/// <summary>
/// Copyright © 2019 Anton Valkenberg
/// Written by BSc. A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by MSc. G.-J. Roelofs, MSc. T. Aliyev and MSc. D. de Rydt.
/// </summary>
namespace AVThesis.SabberStone.Bots {

    /// <summary>
    /// Contains constants used in the SabberStone bots.
    /// </summary>
    public static class Constants {

        #region General

        /// <summary>
        /// The default setting for when the e-greedy strategy should exploit the best action (i.e. be greedy).
        /// Note: the chance of selecting greedily is 1 minus this threshold.
        /// </summary>
        public const double DEFAULT_E_GREEDY_THRESHOLD = 0.2;

        /// <summary>
        /// The default value for the `C' constant in the UCB1 formula.
        /// Note: this is a rounded value of (1 / SquareRoot(2)).
        /// </summary>
        public const double DEFAULT_UCB1_C = 0.707;

        /// <summary>
        /// The default cutoff for amount of turns simulated during playout.
        /// </summary>
        public const int DEFAULT_PLAYOUT_TURN_CUTOFF = 3;

        /// <summary>
        /// The default amount of milliseconds for a budget based on time.
        /// </summary>
        public const long DEFAULT_COMPUTATION_TIME_BUDGET = 10000;

        /// <summary>
        /// The default amount of iterations for a budget based on iterations.
        /// </summary>
        public const int DEFAULT_COMPUTATION_ITERATION_BUDGET = 10000;

        #endregion

        #region LSI

        /// <summary>
        /// The default percentage of the computation budget of LSI that is used during the generation phase.
        /// </summary>
        public const double DEFAULT_LSI_BUDGET_GENERATION_PERCENTAGE = 0.25;

        /// <summary>
        /// The default adjustment factor applied to the evaluation budget for LSI.
        /// Note: this factor should be empirically determined due to the nature of the SequentialHalving algorithm used during LSI evaluation phase.
        /// </summary>
        public const double DEFAULT_LSI_EVALUATION_SAMPLES_ADJUSTMENT_FACTOR = .4;

        #endregion

        #region MCTS

        /// <summary>
        /// The default minimum amount of times a node has to be visited before it can be expanded in MCTS.
        /// </summary>
        public const int DEFAULT_MCTS_MINIMUM_VISIT_THRESHOLD_FOR_EXPANSION = 20;

        /// <summary>
        /// The default minimum number of visits before using the node evaluation to select the best node in MCTS.
        /// </summary>
        public const int DEFAULT_MCTS_MINIMUM_VISIT_THRESHOLD_FOR_SELECTION = 50;

        #endregion

        #region NMCTS

        /// <summary>
        /// The default setting for the global policy in NMCTS.
        /// </summary>
        public const double DEFAULT_NMCTS_GLOBAL_POLICY = 0;

        /// <summary>
        /// The default setting for the local policy in NMCTS.
        /// </summary>
        public const double DEFAULT_NMCTS_LOCAL_POLICY = 0.75;

        #endregion

    }

}
