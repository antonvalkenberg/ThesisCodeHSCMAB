using AVThesis.Bots;
using AVThesis.Search;
using SabberStoneCore.Tasks.PlayerTasks;

/// <summary>
/// Written by A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by G.J. Roelofs and T. Aliyev.
/// </summary>
namespace AVThesis.SabberStone.Strategies {

    /// <summary>
    /// Strategy to play out a game of SabberStone.
    /// </summary>
    public class PlayoutStrategySabberStone : IPlayoutStrategy<object, SabberStoneState, SabberStoneAction, object, SabberStoneAction> {

        #region Fields

        private ISabberStoneBot _playoutBot;

        #endregion

        #region Properties

        /// <summary>
        /// The bot used during the playout.
        /// </summary>
        public ISabberStoneBot PlayoutBot { get => _playoutBot; set => _playoutBot = value; }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructs a new instance that plays out the game using the specified playoutBot.
        /// </summary>
        /// <param name="playoutBot">The bot that will play out the game.</param>
        public PlayoutStrategySabberStone(ISabberStoneBot playoutBot) {
            PlayoutBot = playoutBot;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Plays a game to its end state. This end state should be determined by the goal strategy in the search's context.
        /// </summary>
        /// <param name="context">The context of the search.</param>
        /// <param name="position">The position from which to play out the game.</param>
        /// <returns>The end position.</returns>
        public SabberStoneState Playout(SearchContext<object, SabberStoneState, SabberStoneAction, object, SabberStoneAction> context, SabberStoneState position) {
            // Play out the game.
            while (!context.Goal.Done(context, position)) {
                PlayPlayerTurn(position);
            }
            return position;
        }

        #endregion
        
        #region Private Methods

        /// <summary>
        /// Plays out a player's turn.
        /// Note: this method continously asks the playoutbot to Act and stops when 'null' is returned.
        /// </summary>
        /// <param name="game">The current game state.</param>
        private void PlayPlayerTurn(SabberStoneState game) {

            // Set the correct Controller for the playout bot.
            PlayoutBot.SetController(game.CurrentPlayer() == game.Player1.Id ? game.Player1 : game.Player2);

            // Ask the bot to act.
            var action = PlayoutBot.Act(game);

            // Check if the action is valid.
            if (action.IsComplete()) {

                // Process each task.
                foreach (var item in action.Tasks) {
                    game.Game.Process(item);
                }
            }
            // If not, just pass the turn.
            else {
                game.Game.Process(EndTurnTask.Any(game.Game.CurrentPlayer));
            }
            
        }

        #endregion

    }
}
