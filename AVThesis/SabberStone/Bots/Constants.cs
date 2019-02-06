/// <summary>
/// Written by A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by G.J. Roelofs and T. Aliyev.
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

        #endregion

        #region LSI

        /// <summary>
        /// The default amount of samples LSI is budgeted to use during the generation phase.
        /// </summary>
        public const int DEFAULT_LSI_SAMPLES_FOR_GENERATION = 2500;

        /// <summary>
        /// The default amount of samples LSI is budgeted to use during the evaluation phase.
        /// </summary>
        public const int DEFAULT_LSI_SAMPLES_FOR_EVALUATION = 7500;

        /// <summary>
        /// The default adjustment factor applied to the evaluation budget for LSI.
        /// Note: this factor has to be empirically determined due to the nature of the SequentialHalving algorithm used during LSI evaluation phase.
        /// </summary>
        public const double DEFAULT_LSI_EVALUATION_SAMPLES_ADJUSTMENT_FACTOR = .4;

        #endregion

        #region MCTS

        /// <summary>
        /// The default budget of iterations for MCTS.
        /// </summary>
        public const int DEFAULT_MCTS_ITERATIONS = 10000;

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
        /// The default budget of iterations for NMCTS.
        /// </summary>
        public const int DEFAULT_NMCTS_ITERATIONS = 10000;

        /// <summary>
        /// The default setting for the global policy in NMCTS.
        /// </summary>
        public const double DEFAULT_NMCTS_GLOBAL_POLICY = 0.2;

        /// <summary>
        /// The default setting for the local policy in NMCTS.
        /// </summary>
        public const double DEFAULT_NMCTS_LOCAL_POLICY = 0.2;

        #endregion

    }

}
