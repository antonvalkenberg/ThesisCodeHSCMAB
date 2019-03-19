/// <summary>
/// Written by A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by G.J. Roelofs and T. Aliyev.
/// </summary>
// ReSharper disable InconsistentNaming
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

        /// <summary>
        /// H-MCTS bot with a Turn-Cutoff setting of 2.
        /// </summary>
        HMCTS_TC2,

        /// <summary>
        /// N-MCTS bot with a Turn-Cutoff setting of 2.
        /// </summary>
        NMCTS_TC2,

        /// <summary>
        /// LSI bot with a Turn-Cutoff setting of 2.
        /// </summary>
        LSI_TC2,

        /// <summary>
        /// H-MCTS bot with a Turn-Cutoff setting of 4.
        /// </summary>
        HMCTS_TC4,

        /// <summary>
        /// N-MCTS bot with a Turn-Cutoff setting of 4.
        /// </summary>
        NMCTS_TC4,

        /// <summary>
        /// LSI bot with a Turn-Cutoff setting of 4.
        /// </summary>
        LSI_TC4,

        /// <summary>
        /// H-MCTS bot with an iteration budget of 1000.
        /// </summary>
        HMCTS_IT1K,

        /// <summary>
        /// N-MCTS bot with an iteration budget of 1000.
        /// </summary>
        NMCTS_IT1K,

        /// <summary>
        /// LSI bot with an iteration budget of 1000.
        /// </summary>
        LSI_IT1K,

        /// <summary>
        /// H-MCTS bot with an iteration budget of 5000.
        /// </summary>
        HMCTS_IT5K,

        /// <summary>
        /// N-MCTS bot with an iteration budget of 5000.
        /// </summary>
        NMCTS_IT5K,

        /// <summary>
        /// LSI bot with an iteration budget of 5000.
        /// </summary>
        LSI_IT5K,

        /// <summary>
        /// H-MCTS bot with a time budget of 5 seconds.
        /// </summary>
        HMCTS_TI5S,

        /// <summary>
        /// N-MCTS bot with a time budget of 5 seconds.
        /// </summary>
        NMCTS_TI5S,

        /// <summary>
        /// LSI bot with a time budget of 5 seconds.
        /// </summary>
        LSI_TI5S,
    }

}
