/// <summary>
/// Written by A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by G.J. Roelofs and T. Aliyev.
/// </summary>
namespace AVThesis.Search {

    /// <summary>
    /// Represents the world state of a game.
    /// </summary>
    public abstract class State {

        #region Constants

        /// <summary>
        /// A constant value that indicates that no player has won the game.
        /// </summary>
        public const int DRAW = -1;

        #endregion

        #region Fields

        private int _playerWon = DRAW;
        private bool _primaryPlayer = false;

        #endregion

        #region Properties

        /// <summary>
        /// Which player has won. Note: defaults to <see cref="State.DRAW"/>.
        /// </summary>
        public int PlayerWon { get => _playerWon; set => _playerWon = value; }
        /// <summary>
        /// Whether or not the primary player is the active player.
        /// </summary>
        public bool PrimaryPlayer { get => _primaryPlayer; set => _primaryPlayer = value; }

        #endregion

        #region Public Methods

        /// <summary>
        /// Copies this State and returns it.
        /// </summary>
        /// <returns>New State that is a copy of this State.</returns>
        public abstract dynamic Copy();

        /// <summary>
        /// Returns the unique identifier of the currently active player.
        /// </summary>
        /// <returns>The unique identifier of the currently active player.</returns>
        public abstract int CurrentPlayer();

        /// <summary>
        /// Returns the State's hash code.
        /// </summary>
        /// <returns>The State's hash code.</returns>
        public abstract long HashMethod();

        /// <summary>
        /// Returns the number of players in the game.
        /// </summary>
        /// <returns>The number of players in the game.</returns>
        public abstract int NumberOfPlayers();

        #endregion

    }

}
