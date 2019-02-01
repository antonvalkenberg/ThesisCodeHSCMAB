using System;
using System.Collections.Generic;
using AVThesis.SabberStone.Bots;
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
    public class PlayoutStrategySabberStone : IPlayoutStrategy<List<SabberStoneAction>, SabberStoneState, SabberStoneAction, object, SabberStoneAction> {

        #region Constants

        /// <summary>
        /// The unique identifier for the default bot to use during a playout.
        /// </summary>
        private const int DEFAULT_PLAYOUTBOT_ID = -1;

        #endregion

        #region Events

        /// <summary>
        /// The event that is triggered when a simulation is completed.
        /// </summary>
        public event EventHandler<SimulationCompletedEventArgs> SimulationCompleted;

        /// <summary>
        /// Invokes the EventHandler when a simulation is completed.
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnSimulationCompleted(SimulationCompletedEventArgs e) {
            SimulationCompleted?.Invoke(this, e);
        }

        /// <inheritdoc />
        /// <summary>
        /// Represents the data sent with the event of a simulation being completed.
        /// </summary>
        public class SimulationCompletedEventArgs : EventArgs {
            public SearchContext<List<SabberStoneAction>, SabberStoneState, SabberStoneAction, object, SabberStoneAction> Context { get; set; }
            public SabberStoneState EndState { get; set; }
            public SimulationCompletedEventArgs(SearchContext<List<SabberStoneAction>, SabberStoneState, SabberStoneAction, object, SabberStoneAction> context, SabberStoneState endState) {
                Context = context;
                EndState = endState;
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// The bots to be used during the playout, indexed by PlayerId
        /// </summary>
        public Dictionary<int, ISabberStoneBot> Bots { get; set; }

        #endregion

        #region Constructor

        public PlayoutStrategySabberStone() {
            // Create a default playout bot
            Bots = new Dictionary<int, ISabberStoneBot> {{ DEFAULT_PLAYOUTBOT_ID, new RandomBot()}};
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Adds a bot to this PlayoutStrategy to be used for when a Controller with a specific Id is the current player.
        /// </summary>
        /// <param name="controllerId">The unique identifier of the Controller for which the provided bot should be used.</param>
        /// <param name="bot">The bot that should be used during playout.</param>
        public void AddPlayoutBot(int controllerId, ISabberStoneBot bot) {
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

            // Trigger the SimulationCompleted event.
            OnSimulationCompleted(new SimulationCompletedEventArgs(context, position));

            return position;
        }

        #endregion
        
        #region Private Methods

        /// <summary>
        /// Plays out a player's turn.
        /// Note: this method continously asks the playoutbots to Act and stops when 'null' is returned.
        /// </summary>
        /// <param name="game">The current game state.</param>
        private void PlayPlayerTurn(SabberStoneState game) {

            // Select the correct playoutBot to use.
            ISabberStoneBot turnBot;
            if (Bots.ContainsKey(game.CurrentPlayer())) turnBot = Bots[game.CurrentPlayer()];
            else {
                turnBot = Bots[DEFAULT_PLAYOUTBOT_ID];
                turnBot.SetController(game.Game.CurrentPlayer);
            }

            // Ask the bot to act.
            var action = turnBot.Act(game);

            // Check if the action is valid.
            if (action.IsComplete()) {

                // Process each task.
                foreach (var item in action.Tasks) {
                    game.Game.Process(item.Task);
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
