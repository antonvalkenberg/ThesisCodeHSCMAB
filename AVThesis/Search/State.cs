﻿/// <summary>
/// Copyright © 2019 Anton Valkenberg
/// Written by BSc. A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by MSc. G.-J. Roelofs, MSc. T. Aliyev and MSc. D. de Rydt.
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

        #region Properties

        /// <summary>
        /// Which player has won. Note: defaults to <see cref="State.DRAW"/>.
        /// </summary>
        public int PlayerWon { get; set; } = DRAW;

        /// <summary>
        /// Whether or not the primary player is the active player.
        /// </summary>
        public bool PrimaryPlayer { get; set; }

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
        /// Whether or not this State is equal to the argument State.
        /// </summary>
        /// <param name="otherState">The State to check with this State for equality.</param>
        /// <returns>Boolean indicating whether or not the argument State is equal to this State.</returns>
        public abstract bool Equals(State otherState);

        /// <summary>
        /// Returns the State's hash code.
        /// </summary>
        /// <returns>The State's hash code.</returns>
        public abstract int HashMethod();

        /// <summary>
        /// Returns the number of players in the game.
        /// </summary>
        /// <returns>The number of players in the game.</returns>
        public abstract int NumberOfPlayers();

        /// <summary>
        /// Whether or not this state represents a terminal state.
        /// </summary>
        /// <returns></returns>
        public abstract bool IsTerminal();

        #endregion

    }

}
