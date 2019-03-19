using System.Linq;
using AVThesis.Datastructures;
using SabberStoneCore.Enums;
using SabberStoneCore.Model.Entities;
using SabberStoneCore.Tasks.PlayerTasks;

/// <summary>
/// Written by A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by G.J. Roelofs and T. Aliyev.
/// </summary>
namespace AVThesis.SabberStone.Bots {

    /// <summary>
    /// A bot that plays Hearthstone through random moves.
    /// </summary>
    public class RandomBot : ISabberStoneBot {

        #region Constants

        private const string BOT_NAME = "RandomBot";

        #endregion

        #region Properties

        /// <summary>
        /// The player this bot represents.
        /// </summary>
        public Controller Player { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructs a new instance of RandomBot with a <see cref="SabberStoneCore.Model.Entities.Controller"/> representing the player.
        /// </summary>
        /// <param name="player">The player.</param>
        public RandomBot(Controller player) {
            Player = player;
        }

        /// <summary>
        /// Constructs a new instance of RandomBot without a set player.
        /// </summary>
        public RandomBot() {
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Creates a SabberStoneAction by randomly selecting one of the available PlayerTasks until the End_Turn task is selected.
        /// </summary>
        /// <param name="state">The game state for which an action should be created. Note: </param>
        /// <param name="filterDuplicatePositionTasks">[Optional] Whether or not to filter out excess positioning on playing cards. Default value is false.</param>
        /// <returns>SabberStoneAction</returns>
        public SabberStoneAction CreateRandomAction(SabberStoneState state, bool filterDuplicatePositionTasks = false) {
            // Clone game so that we can process the selected tasks and get an updated options list.
            var clonedGame = state.Game.Clone();
            var playerID = clonedGame.CurrentPlayer.Id;

            // Create an action to store the selected tasks.
            var action = new SabberStoneAction();

            // Keep adding random tasks until the player's turn is over, or the game has ended
            while (clonedGame.CurrentPlayer.Id == playerID && clonedGame.State != State.COMPLETE) {
                // Check if an duplicate positions need to be filtered out
                var availableOptions = clonedGame.CurrentPlayer.Options();
                if (filterDuplicatePositionTasks)
                    availableOptions = availableOptions.Where(i => i.ZonePosition <= 0).ToList();
                // Select a random available task
                var selectedTask = availableOptions.RandomElementOrDefault();
                // Add the task to the action.
                action.AddTask((SabberStonePlayerTask)selectedTask);
                // Process the task on the cloned game state.
                clonedGame.Process(selectedTask);
            }

            // Check if the action is complete, if not, add EndTurn
            if (!action.IsComplete()) {
                action.AddTask((SabberStonePlayerTask)EndTurnTask.Any(clonedGame.CurrentPlayer));
            }

            return action;
        }

        /// <summary>
        /// Returns a SabberStoneAction for the current state.
        /// Note: If this player has no available options, null is returned.
        /// </summary>
        /// <param name="state">The current game state.</param>
        /// <returns>SabberStoneAction or null in the case of no available options.</returns>
        public SabberStoneAction Act(SabberStoneState state) {
            // Check to make sure the player to act in the game-state matches our player.
            if (state.CurrentPlayer() != Player.Id) {
                return null;
            }

            // Check if there are any options, otherwise return a randomly created action.
            return Player.Options().IsNullOrEmpty() ? SabberStoneAction.CreateNullMove(Player) : CreateRandomAction(state);
        }

        /// <summary>
        /// Sets the Controller that the bot represents within a SabberStone Game.
        /// </summary>
        /// <param name="controller">This bot's Controller.</param>
        public void SetController(Controller controller) {
            Player = controller;
        }

        /// <summary>
        /// Returns the player's ID.
        /// </summary>
        /// <returns>Integer representing the player's ID.</returns>
        public int PlayerID() {
            return Player.Id;
        }

        /// <summary>
        /// Returns the Bot's name.
        /// </summary>
        /// <returns>String representing the Bot's name.</returns>
        public string Name() {
            return Player == null ? BOT_NAME : $"{BOT_NAME}-{PlayerID()}";
        }

        /// <inheritdoc />
        public long BudgetSpent() {
            return 0;
        }

        #endregion

    }
}
