/// <summary>
/// Written by A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by G.J. Roelofs and T. Aliyev.
/// </summary>
namespace AVThesis.Search {

    /// <summary>
    /// Collection of constants used throughout the Search namespace.
    /// </summary>
    public static class Constants {

        /// <summary>
        /// Whether or not the search should be limited on an amount of iterations.
        /// </summary>
        public const int NO_LIMIT_ON_ITERATIONS = -1;

        /// <summary>
        /// Whether or not the search should be limited on its running time.
        /// </summary>
        public const int NO_LIMIT_ON_THINKING_TIME = -1;

        /// <summary>
        /// The default chance to explore vesus exploit.
        /// </summary>
        public const double DEFAULT_EXPLORE_CHANCE = 0.5;

    }

}
