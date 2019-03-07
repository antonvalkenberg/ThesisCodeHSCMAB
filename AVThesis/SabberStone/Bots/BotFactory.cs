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
                default:
                    throw new InvalidEnumArgumentException($"BotSetupType `{botType}' is not supported.");
            }
        }

    }

}
