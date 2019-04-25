using System.Collections.Generic;
using AVThesis.SabberStone.Bots;
using AVThesis.Search;

/// <summary>
/// Copyright © 2019 Anton Valkenberg
/// Written by BSc. A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by MSc. G.-J. Roelofs, MSc. T. Aliyev and MSc. D. de Rydt.
/// </summary>
namespace AVThesis.SabberStone.Strategies {

    /// <summary>
    /// Strategy to play out a game of SabberStone.
    /// </summary>
    public class PlayoutStrategySabberStone : IPlayoutStrategy<List<SabberStoneAction>, SabberStoneState, SabberStoneAction, object, SabberStoneAction> {

        #region Constants

        /// <summary>
        /// The unique identifier for the default bot to use during a playout.
        /// </summary>
        private const int DEFAULT_PLAYOUT_BOT_ID = -1;

        #endregion

        #region Properties

        /// <summary>
        /// The bots to be used during the playout, indexed by PlayerId
        /// </summary>
        public Dictionary<int, IPlayoutBot> Bots { get; set; }

        #endregion

        #region Constructor

        public PlayoutStrategySabberStone() {
            // Create a default playout bot
            Bots = new Dictionary<int, IPlayoutBot> {{ DEFAULT_PLAYOUT_BOT_ID, new RandomBot()}};
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds a bot to this PlayoutStrategy to be used for when a Controller with a specific Id is the current player.
        /// </summary>
        /// <param name="controllerId">The unique identifier of the Controller for which the provided bot should be used.</param>
        /// <param name="bot">The bot that should be used during playout.</param>
        public void AddPlayoutBot(int controllerId, IPlayoutBot bot) {
            if (!Bots.ContainsKey(controllerId)) Bots.Add(controllerId, null);
            Bots[controllerId] = bot;
        }

        /// <summary>
        /// Plays a game to its end state. This end state should be determined by the goal strategy in the search's context.
        /// </summary>
        /// <param name="context">The context of the search.</param>
        /// <param name="position">The position from which to play out the game.</param>
        /// <returns>The end position.</returns>
        public SabberStoneState Playout(SearchContext<List<SabberStoneAction>, SabberStoneState, SabberStoneAction, object, SabberStoneAction> context, SabberStoneState position) {
            // Play out the game as long as the Goal strategy dictates.
            while (!context.Goal.Done(context, position)) {
                PlayPlayerTurn(position);
            }

            // Tell the playout bot(s) that the playout has completed.
            foreach (var playoutBot in Bots) {
                playoutBot.Value.PlayoutCompleted(context, position);
            }

            return position;
        }

        #endregion
        
        #region Private Methods

        /// <summary>
        /// Plays out a player's turn.
        /// Note: this method asks the playout bot of the current player to Act and processes the returned action.
        /// </summary>
        /// <param name="game">The current game state.</param>
        private void PlayPlayerTurn(SabberStoneState game) {

            // Select the correct playoutBot to use.
            IPlayoutBot turnBot;
            if (Bots.ContainsKey(game.CurrentPlayer())) turnBot = Bots[game.CurrentPlayer()];
            else {
                // Fall back to the default playout bot if the current player does not have a playout bot specified.
                turnBot = Bots[DEFAULT_PLAYOUT_BOT_ID];
                turnBot.SetController(game.Game.CurrentPlayer);
            }

            // Ask the bot to act.
            var action = turnBot.Act(game);
            
            // Process each task.
            foreach (var item in action.Tasks) {
                game.Game.Process(item.Task);
            }
            
        }

        #endregion

    }

}
