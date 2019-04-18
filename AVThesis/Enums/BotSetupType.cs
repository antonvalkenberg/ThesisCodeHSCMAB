/// <summary>
/// Written by A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by G.J. Roelofs and T. Aliyev.
/// </summary>
// ReSharper disable InconsistentNaming

using System.Diagnostics.CodeAnalysis;

namespace AVThesis.Enums {

    /// <summary>
    /// Enumeration of types of setups for <see cref="SabberStone.ISabberStoneBot"/>.
    /// </summary>
    public enum BotSetupType {

        #region Default settings

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

        #endregion

        #region Playout length test

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

        #endregion

        #region Iteration budget test

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

        #endregion

        #region Time budget test

        /// <summary>
        /// H-MCTS bot with the default time budget.
        /// </summary>
        HMCTS_TI,

        /// <summary>
        /// N-MCTS bot with the default time budget.
        /// </summary>
        NMCTS_TI,

        /// <summary>
        /// LSI bot with the default time budget.
        /// </summary>
        LSI_TI,

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

        #endregion

        #region Timed playout length test

        /// <summary>
        /// H-MCTS bot with a Turn-Cutoff setting of 2 and a time budget of 5 seconds.
        /// </summary>
        HMCTS_TC2_TI5S,

        /// <summary>
        /// H-MCTS bot with a Turn-Cutoff setting of 4 and a time budget of 5 seconds.
        /// </summary>
        HMCTS_TC4_TI5S,

        /// <summary>
        /// N-MCTS bot with a Turn-Cutoff setting of 2 and a time budget of 5 seconds.
        /// </summary>
        NMCTS_TC2_TI5S,

        /// <summary>
        /// N-MCTS bot with a Turn-Cutoff setting of 4 and a time budget of 5 seconds.
        /// </summary>
        NMCTS_TC4_TI5S,

        /// <summary>
        /// LSI bot with a Turn-Cutoff setting of 2 and a time budget of 5 seconds.
        /// </summary>
        LSI_TC2_TI5S,

        /// <summary>
        /// LSI bot with a Turn-Cutoff setting of 4 and a time budget of 5 seconds.
        /// </summary>
        LSI_TC4_TI5S,

        #endregion

        #region H-MCTS C constant test

        /// <summary>
        /// H-MCTS bot with a UCT-c-constant setting of 0.2 and a time budget of 5 seconds.
        /// </summary>
        HMCTS_C02_TI5S,

        /// <summary>
        /// H-MCTS bot with a UCT-c-constant setting of 0.5 and a time budget of 5 seconds.
        /// </summary>
        HMCTS_C05_TI5S,

        #endregion

        #region H-MCTS Visit threshold test

        /// <summary>
        /// H-MCTS bot with a minimum visit threshold of 0 for selection and a time budget of 5 seconds.
        /// </summary>
        HMCTS_SELECT_0_TI5S,

        /// <summary>
        /// H-MCTS bot with a minimum visit threshold of 0 for expansion and a time budget of 5 seconds.
        /// </summary>
        HMCTS_EXPAND_0_TI5S,

        /// <summary>
        /// H-MCTS bot with a minimum visit threshold of 25 for expansion and a time budget of 5 seconds.
        /// </summary>
        HMCTS_EXPAND_25_TI5S,

        #endregion

        #region Playout strategy test

        /// <summary>
        /// H-MCTS bot with MAST playout bots using UBC1 selection and a time budget of 5 seconds.
        /// </summary>
        HMCTS_MAST_UCB_TI5S,

        /// <summary>
        /// N-MCTS bot with MAST playout bots using UCB1 selection and a time budget of 5 seconds.
        /// </summary>
        NMCTS_MAST_UCB_TI5S,

        /// <summary>
        /// LSI bot with MAST playout bots using UCB1 selection and a time budget of 5 seconds.
        /// </summary>
        LSI_MAST_UCB_TI5S,

        /// <summary>
        /// H-MCTS bot with Random playout bots and a time budget of 5 seconds.
        /// </summary>
        HMCTS_RNG_PLAYOUT_TI5S,

        /// <summary>
        /// N-MCTS bot with Random playout bots and a time budget of 5 seconds.
        /// </summary>
        NMCTS_RNG_PLAYOUT_TI5S,

        /// <summary>
        /// LSI bot with Random playout bots and a time budget of 5 seconds.
        /// </summary>
        LSI_RNG_PLAYOUT_TI5S,

        #endregion

        #region LSI generation vs evaluation budget test

        /// <summary>
        /// LSI bot that uses 50% of its budget in the generation phase and a time budget of 5 seconds.
        /// </summary>
        LSI_05GEN_TI5S,

        /// <summary>
        /// LSI bot that uses 75% of its budget in the generation phase and a time budget of 5 seconds.
        /// </summary>
        LSI_075GEN_TI5S,

        #endregion

        #region N-MCTS global and local policy test

        /// <summary>
        /// N-MCTS bot with a global policy of 0.1 and a time budget of 5 seconds.
        /// </summary>
        NMCTS_01GLOBAL_TI5S,

        /// <summary>
        /// N-MCTS bot with a global policy of 0.33 and a time budget of 5 seconds.
        /// </summary>
        NMCTS_033GLOBAL_TI5S,

        /// <summary>
        /// N-MCTS bot with a global policy of 0.50 and a time budget of 5 seconds.
        /// </summary>
        NMCTS_050GLOBAL_TI5S,

        /// <summary>
        /// N-MCTS bot with a global policy of 0.75 and a time budget of 5 seconds.
        /// </summary>
        NMCTS_075GLOBAL_TI5S,

        /// <summary>
        /// N-MCTS bot with a local policy of 0.25 and a time budget of 5 seconds.
        /// </summary>
        NMCTS_025LOCAL_TI5S,

        /// <summary>
        /// N-MCTS bot with a local policy of 0.5 and a time budget of 5 seconds.
        /// </summary>
        NMCTS_05LOCAL_TI5S,

        #endregion

        #region Ensemble size test

        /// <summary>
        /// H-MCTS bot with an ensemble size of 2 and a time budget of 5 seconds.
        /// </summary>
        HMCTS_ES2_TI5S,

        /// <summary>
        /// H-MCTS bot with an ensemble size of 5 and a time budget of 5 seconds.
        /// </summary>
        HMCTS_ES5_TI5S,

        /// <summary>
        /// N-MCTS bot with an ensemble size of 2 and a time budget of 5 seconds.
        /// </summary>
        NMCTS_ES2_TI5S,

        /// <summary>
        /// N_MCTS bot with an ensemble size of 5 and a time budget of 5 seconds.
        /// </summary>
        NMCTS_ES5_TI5S,

        /// <summary>
        /// LSI bot with an ensemble size of 2 and a time budget of 5 seconds.
        /// </summary>
        LSI_ES2_TI5S,

        /// <summary>
        /// LSI bot with an ensemble size of 5 and a time budget of 5 seconds.
        /// </summary>
        LSI_ES5_TI5S,

        #endregion

        #region H-MCTS Dimensional Ordering test

        /// <summary>
        /// H-MCTS bot with optimal settings, dimensional ordering by ascending entropy and a time budget of 5 seconds.
        /// </summary>
        HMCTS_Opt_EntAsc_Ti5s,

        /// <summary>
        /// H-MCTS bot with optimal settings, dimensional ordering by descending entropy and a time budget of 5 seconds.
        /// </summary>
        HMCTS_Opt_EntDesc_Ti5s,

        /// <summary>
        /// H-MCTS bot with optimal settings, dimensional ordering by average evaluation and a time budget of 5 seconds.
        /// </summary>
        [SuppressMessage("ReSharper", "IdentifierTypo")]
        HMCTS_Opt_AvgEval_Ti5s,

        /// <summary>
        /// H-MCTS bot with optimal settings, dimensional ordering by task type and a time budget of 5 seconds.
        /// </summary>
        HMCTS_Opt_TaskType_Ti5s,

        /// <summary>
        /// H-MCTS bot with optimal settings, dimensional ordering by descending mana cost and a time budget of 5 seconds.
        /// </summary>
        HMCTS_Opt_ManaDesc_Ti5s,

        /// <summary>
        /// H-MCTS bot with optimal settings, no dimensional ordering and a time budget of 5 seconds.
        /// </summary>
        HMCTS_Opt_NoOrder_Ti5s,

        #endregion

        #region LSI Budget Estimation test

        /// <summary>
        /// LSI bot that uses the previous search to estimate its the number of samples it can run in its time budget of 5 seconds.
        /// </summary>
        LSI_PrevSearch_Ti5s,

        #endregion

    }

}
