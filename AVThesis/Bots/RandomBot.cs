using AVThesis.SabberStone;
using SabberStoneCore.Model.Entities;
using AVThesis.Datastructures;

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
        /// Returns a SabberStoneAction for the current state.
        /// Note: If this player has no available options, null is returned.
        /// </summary>
        /// <param name="state">The current game state.</param>
        /// <returns>SabberStoneAction or null in the case of no available options.</returns>
        public SabberStoneAction Act(SabberStoneState state) {
            // Check to make sure the player to act in the gamestate matches our player.
            if (state.CurrentPlayer() != Player.Id) {
                //TODO throw an exception?
                return null;
            }

            // Check if there are any options.
            if (Player.Options().IsNullOrEmpty()) return null;

            // Select and return a random option.
            return new SabberStoneAction(Player.Options().RandomElementOrDefault());
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
