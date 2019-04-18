using System.ComponentModel;
using AVThesis.Enums;
using SabberStoneCore.Model.Entities;

/// <summary>
/// Written by A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by G.J. Roelofs and T. Aliyev.
/// </summary>
namespace AVThesis.SabberStone.Bots {

    /// <summary>
    /// Holds various setups for bots and allows the creation of instances of them.
    /// </summary>
    public static class BotFactory {

        /// <summary>
        /// Creates an <see cref="ISabberStoneBot"/> instance of the specified bot type and set the provided <see cref="Controller"/> in the bot.
        /// </summary>
        /// <param name="botType">The type of bot to create.</param>
        /// <param name="player">The <see cref="Controller"/> to set in the bot.</param>
        /// <returns><see cref="ISabberStoneBot"/></returns>
        public static ISabberStoneBot CreateSabberStoneBot(BotSetupType botType, Controller player) {
            var bot = CreateSabberStoneBot(botType);
            bot.SetController(player);
            return bot;
        }

        /// <summary>
        /// Creates an <see cref="ISabberStoneBot"/> instance of the specified bot type without a <see cref="Controller"/>.
        /// </summary>
        /// <param name="botType">The type of bot to create.</param>
        /// <returns><see cref="ISabberStoneBot"/></returns>
        public static ISabberStoneBot CreateSabberStoneBot(BotSetupType botType) {
            switch (botType) {
                case BotSetupType.RandomBot:
                    return new RandomBot();
                case BotSetupType.HeuristicBot:
                    return new HeuristicBot();
                case BotSetupType.DefaultHMCTS:
                    return new HMCTSBot();
                case BotSetupType.DefaultNMCTS:
                    return new NMCTSBot();
                case BotSetupType.DefaultLSI:
                    return new LSIBot();
                case BotSetupType.HMCTS_TC2:
                    return new HMCTSBot(playoutTurnCutoff: 2);
                case BotSetupType.NMCTS_TC2:
                    return new NMCTSBot(playoutTurnCutoff: 2);
                case BotSetupType.LSI_TC2:
                    return new LSIBot(playoutTurnCutoff: 2);
                case BotSetupType.HMCTS_TC4:
                    return new HMCTSBot(playoutTurnCutoff: 4);
                case BotSetupType.NMCTS_TC4:
                    return new NMCTSBot(playoutTurnCutoff: 4);
                case BotSetupType.LSI_TC4:
                    return new LSIBot(playoutTurnCutoff: 4);
                case BotSetupType.HMCTS_IT1K:
                    return new HMCTSBot(iterations: 1000);
                case BotSetupType.NMCTS_IT1K:
                    return new NMCTSBot(iterations: 1000);
                case BotSetupType.LSI_IT1K:
                    return new LSIBot(samples: 1000);
                case BotSetupType.HMCTS_IT5K:
                    return new HMCTSBot(iterations: 5000);
                case BotSetupType.NMCTS_IT5K:
                    return new NMCTSBot(iterations: 5000);
                case BotSetupType.LSI_IT5K:
                    return new LSIBot(samples: 5000);
                case BotSetupType.HMCTS_TI5S:
                    return new HMCTSBot(budgetType: BudgetType.Time, time: 5000);
                case BotSetupType.NMCTS_TI5S:
                    return new NMCTSBot(budgetType: BudgetType.Time, time: 5000);
                case BotSetupType.LSI_TI5S:
                    return new LSIBot(budgetType: BudgetType.Time, time: 5000);
                case BotSetupType.HMCTS_TI:
                    return new HMCTSBot(budgetType: BudgetType.Time);
                case BotSetupType.NMCTS_TI:
                    return new NMCTSBot(budgetType: BudgetType.Time);
                case BotSetupType.LSI_TI:
                    return new LSIBot(budgetType: BudgetType.Time);
                case BotSetupType.HMCTS_TC2_TI5S:
                    return new HMCTSBot(playoutTurnCutoff: 2, budgetType: BudgetType.Time, time: 5000);
                case BotSetupType.HMCTS_TC4_TI5S:
                    return new HMCTSBot(playoutTurnCutoff: 4, budgetType: BudgetType.Time, time: 5000);
                case BotSetupType.NMCTS_TC2_TI5S:
                    return new NMCTSBot(playoutTurnCutoff: 2, budgetType: BudgetType.Time, time: 5000);
                case BotSetupType.NMCTS_TC4_TI5S:
                    return new NMCTSBot(playoutTurnCutoff: 4, budgetType: BudgetType.Time, time: 5000);
                case BotSetupType.LSI_TC2_TI5S:
                    return new LSIBot(playoutTurnCutoff: 2, budgetType: BudgetType.Time, time: 5000);
                case BotSetupType.LSI_TC4_TI5S:
                    return new LSIBot(playoutTurnCutoff: 4, budgetType: BudgetType.Time, time: 5000);
                case BotSetupType.HMCTS_C02_TI5S:
                    return new HMCTSBot(ucbConstantC: 0.2, budgetType: BudgetType.Time, time: 5000);
                case BotSetupType.HMCTS_C05_TI5S:
                    return new HMCTSBot(ucbConstantC: 0.5, budgetType: BudgetType.Time, time: 5000);
                case BotSetupType.HMCTS_MAST_UCB_TI5S:
                    return new HMCTSBot(mastSelectionType: MASTPlayoutBot.SelectionType.UCB, budgetType: BudgetType.Time, time: 5000);
                case BotSetupType.NMCTS_MAST_UCB_TI5S:
                    return new NMCTSBot(mastSelectionType: MASTPlayoutBot.SelectionType.UCB, budgetType: BudgetType.Time, time: 5000);
                case BotSetupType.LSI_MAST_UCB_TI5S:
                    return new LSIBot(mastSelectionType: MASTPlayoutBot.SelectionType.UCB, budgetType: BudgetType.Time, time: 5000);
                case BotSetupType.HMCTS_RNG_PLAYOUT_TI5S:
                    return new HMCTSBot(mastSelectionType: MASTPlayoutBot.SelectionType.Random, budgetType: BudgetType.Time, time: 5000);
                case BotSetupType.NMCTS_RNG_PLAYOUT_TI5S:
                    return new NMCTSBot(mastSelectionType: MASTPlayoutBot.SelectionType.Random, budgetType: BudgetType.Time, time: 5000);
                case BotSetupType.LSI_RNG_PLAYOUT_TI5S:
                    return new LSIBot(mastSelectionType: MASTPlayoutBot.SelectionType.Random, budgetType: BudgetType.Time, time: 5000);
                case BotSetupType.LSI_05GEN_TI5S:
                    return new LSIBot(generationBudgetPercentage: 0.5, budgetType: BudgetType.Time, time: 5000);
                case BotSetupType.LSI_075GEN_TI5S:
                    return new LSIBot(generationBudgetPercentage: 0.75, budgetType: BudgetType.Time, time: 5000);
                case BotSetupType.NMCTS_01GLOBAL_TI5S:
                    return new NMCTSBot(globalPolicy: 0.1, budgetType: BudgetType.Time, time: 5000);
                case BotSetupType.NMCTS_033GLOBAL_TI5S:
                    return new NMCTSBot(globalPolicy: 0.33, budgetType: BudgetType.Time, time: 5000);
                case BotSetupType.NMCTS_050GLOBAL_TI5S:
                    return new NMCTSBot(globalPolicy: 0.50, budgetType: BudgetType.Time, time: 5000);
                case BotSetupType.NMCTS_075GLOBAL_TI5S:
                    return new NMCTSBot(globalPolicy: 0.75, budgetType: BudgetType.Time, time: 5000);
                case BotSetupType.NMCTS_025LOCAL_TI5S:
                    return new NMCTSBot(localPolicy: 0.25, budgetType: BudgetType.Time, time: 5000);
                case BotSetupType.NMCTS_05LOCAL_TI5S:
                    return new NMCTSBot(localPolicy: 0.5, budgetType: BudgetType.Time, time: 5000);
                case BotSetupType.HMCTS_SELECT_0_TI5S:
                    return new HMCTSBot(minimumVisitThresholdForSelection: 0, budgetType: BudgetType.Time, time: 5000);
                case BotSetupType.HMCTS_EXPAND_0_TI5S:
                    return new HMCTSBot(minimumVisitThresholdForExpansion: 0, budgetType: BudgetType.Time, time: 5000);
                case BotSetupType.HMCTS_EXPAND_25_TI5S:
                    return new HMCTSBot(minimumVisitThresholdForExpansion: 25, budgetType: BudgetType.Time, time: 5000);
                case BotSetupType.HMCTS_ES2_TI5S:
                    return new HMCTSBot(ensembleSize: 2, budgetType: BudgetType.Time, time: 5000);
                case BotSetupType.HMCTS_ES5_TI5S:
                    return new HMCTSBot(ensembleSize: 5, budgetType: BudgetType.Time, time: 5000);
                case BotSetupType.NMCTS_ES2_TI5S:
                    return new NMCTSBot(ensembleSize: 2, budgetType: BudgetType.Time, time: 5000);
                case BotSetupType.NMCTS_ES5_TI5S:
                    return new NMCTSBot(ensembleSize: 5, budgetType: BudgetType.Time, time: 5000);
                case BotSetupType.LSI_ES2_TI5S:
                    return new LSIBot(ensembleSize: 2, budgetType: BudgetType.Time, time: 5000);
                case BotSetupType.LSI_ES5_TI5S:
                    return new LSIBot(ensembleSize: 5, budgetType: BudgetType.Time, time: 5000);
                case BotSetupType.HMCTS_Opt_EntAsc_Ti5s:
                    return new HMCTSBot(dimensionalOrdering: DimensionalOrderingType.EntropyAsc, playoutTurnCutoff: 4, mastSelectionType: MASTPlayoutBot.SelectionType.Random, ucbConstantC: 0.2, minimumVisitThresholdForExpansion: 0, minimumVisitThresholdForSelection: 0, budgetType: BudgetType.Time, time: 5000);
                case BotSetupType.HMCTS_Opt_EntDesc_Ti5s:
                    return new HMCTSBot(dimensionalOrdering: DimensionalOrderingType.EntropyDesc, playoutTurnCutoff: 4, mastSelectionType: MASTPlayoutBot.SelectionType.Random, ucbConstantC: 0.2, minimumVisitThresholdForExpansion: 0, minimumVisitThresholdForSelection: 0, budgetType: BudgetType.Time, time: 5000);
                case BotSetupType.HMCTS_Opt_AvgEval_Ti5s:
                    return new HMCTSBot(dimensionalOrdering: DimensionalOrderingType.Evaluation, playoutTurnCutoff: 4, mastSelectionType: MASTPlayoutBot.SelectionType.Random, ucbConstantC: 0.2, minimumVisitThresholdForExpansion: 0, minimumVisitThresholdForSelection: 0, budgetType: BudgetType.Time, time: 5000);
                case BotSetupType.HMCTS_Opt_TaskType_Ti5s:
                    return new HMCTSBot(dimensionalOrdering: DimensionalOrderingType.TaskType, playoutTurnCutoff: 4, mastSelectionType: MASTPlayoutBot.SelectionType.Random, ucbConstantC: 0.2, minimumVisitThresholdForExpansion: 0, minimumVisitThresholdForSelection: 0, budgetType: BudgetType.Time, time: 5000);
                case BotSetupType.HMCTS_Opt_ManaDesc_Ti5s:
                    return new HMCTSBot(dimensionalOrdering: DimensionalOrderingType.ManaDesc, playoutTurnCutoff: 4, mastSelectionType: MASTPlayoutBot.SelectionType.Random, ucbConstantC: 0.2, minimumVisitThresholdForExpansion: 0, minimumVisitThresholdForSelection: 0, budgetType: BudgetType.Time, time: 5000);
                case BotSetupType.HMCTS_Opt_NoOrder_Ti5s:
                    return new HMCTSBot(dimensionalOrdering: DimensionalOrderingType.None, playoutTurnCutoff: 4, mastSelectionType: MASTPlayoutBot.SelectionType.Random, ucbConstantC: 0.2, minimumVisitThresholdForExpansion: 0, minimumVisitThresholdForSelection: 0, budgetType: BudgetType.Time, time: 5000);
                case BotSetupType.LSI_PrevSearch_Ti5s:
                    return new LSIBot(budgetType: BudgetType.Time, time: 5000, budgetEstimationType: LSIBot.BudgetEstimationType.PreviousSearchAverage);
                default:
                    throw new InvalidEnumArgumentException($"BotSetupType `{botType}' is not supported.");
            }
        }

    }

}
