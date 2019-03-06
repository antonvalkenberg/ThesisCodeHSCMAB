/// <summary>
/// Written by A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by G.J. Roelofs and T. Aliyev.
/// </summary>
namespace AVThesis.Enums {

    /// <summary>
    /// Enumeration of types of setups for <see cref="SabberStone.ISabberStoneBot"/>.
    /// </summary>
    public enum BotSetupType {

        /// <summary>
        /// See <see cref="SabberStone.Bots.RandomBot"/>.
        /// </summary>
        RandomBot,

        /// <summary>
        /// See <see cref="SabberStone.Bots.HeuristicBot"/>.
        /// </summary>
        HeuristicBot,

        /// <summary>
        /// H-MCTS bot with all default settings.
        /// </summary>
        DefaultHMCTS,

        /// <summary>
        /// N-MCTS bot with all default settings.
        /// </summary>
        DefaultNMCTS,

        /// <summary>
        /// LSI bot with all default settings.
        /// </summary>
        DefaultLSI,

    }

}
