using AVThesis.SabberStone;
using SabberStoneCore.Model.Entities;
using AVThesis.Datastructures;
using SabberStoneCore.Tasks;

/// <summary>
/// Written by A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by G.J. Roelofs and T. Aliyev.
/// </summary>
namespace AVThesis.Bots {

    /// <summary>
    /// A bot that plays Hearthstone through random moves.
    /// </summary>
    public class RandomBot : ISabberStoneBot {

        #region Fields

        private const string _botName = "RandomBot";
        private Controller _player;

        #endregion

        #region Properties

        /// <summary>
        /// The player this bot represents.
        /// </summary>
        public Controller Player { get => _player; set => _player = value; }

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
        /// <returns>SabberStoneAction</returns>
        public SabberStoneAction CreateRandomAction(SabberStoneState state) {

            // Clone game so that we can process the selected tasks and get an updated options list.
            var clonedGame = state.Game.Clone();
            var clonedPlayer = clonedGame.CurrentPlayer;

            // Create an action to store the selected tasks.
            var action = new SabberStoneAction();

            // Keep selecting random actions until the 'end turn' task is selected, then stop.
            var selectedTask = clonedPlayer.Options().RandomElementOrDefault();
            do {
                // Add the task to the action.
                action.AddTask(selectedTask);

                // Process the task on the cloned game state.
                clonedGame.Process(selectedTask);

                // select another random option.
                selectedTask = clonedPlayer.Options().RandomElementOrDefault();

                // Keep selecting tasks while we're still the active player, there is something to choose and we haven't chosen to pass the turn.
            } while (clonedGame.CurrentPlayer.Id == clonedPlayer.Id && selectedTask != null && selectedTask.PlayerTaskType != PlayerTaskType.END_TURN);

            // Add the last selected task, if it is not null
            if (selectedTask != null) action.AddTask(selectedTask);

            return action;
        }

        /// <summary>
        /// Returns a SabberStoneAction for the current state.
        /// Note: If this player has no available options, null is returned.
        /// </summary>
        /// <param name="state">The current game state.</param>
        /// <returns>SabberStoneAction or null in the case of no available options.</returns>
        public SabberStoneAction Act(SabberStoneState state) {
            // Check to make sure the player to act in the gamestate matches our player.
            if (state.CurrentPlayer() != Player.Id) {
                return null;
            }

            // Check if there are any options.
            if (Player.Options().IsNullOrEmpty()) return SabberStoneAction.CreateNullMove(Player);

            // Return a randomly created action.
            return CreateRandomAction(state);
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
            return _botName;
        }

        #endregion

    }
}
