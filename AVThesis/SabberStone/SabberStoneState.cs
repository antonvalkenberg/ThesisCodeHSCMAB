using System.Collections.Generic;
using AVThesis.Search;
using SabberStoneCore.Model.Entities;

/// <summary>
/// Written by A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by G.J. Roelofs and T. Aliyev.
/// </summary>
namespace AVThesis.SabberStone {

    /// <summary>
    /// Wrapper class for using a SabberStone Game as a State in the search framework.
    /// </summary>
    public class SabberStoneState : State {

        #region Fields

        private SabberStoneCore.Model.Game _game;

        #endregion

        #region Properties

        /// <summary>
        /// The SabberStone Game.
        /// </summary>
        public SabberStoneCore.Model.Game Game { get => _game; set => _game = value; }
        /// <summary>
        /// The 1st player in the SabberStone Game.
        /// </summary>
        public Controller Player1 => Game.Player1;
        /// <summary>
        /// The 2nd player in the SabberStone Game.
        /// </summary>
        public Controller Player2 => Game.Player2;
        /// <summary>
        /// The ID of the player that has won. Note: defaults to <see cref="State.DRAW"/>.
        /// </summary>
        public new int PlayerWon { get {
                if (Game.State != SabberStoneCore.Enums.State.COMPLETE) return DRAW;
                return Player1.PlayState == SabberStoneCore.Enums.PlayState.WON ? Player1.Id : Player2.Id;
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructs a new instance of a SabberStoneState.
        /// </summary>
        /// <param name="game">The SabberStone Game that the new SabberStoneState should represents.</param>
        public SabberStoneState(SabberStoneCore.Model.Game game) {
            Game = game;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Obfuscates this state by hiding the cards in the HandZone, DeckZone and SecretZone of the specified player.
        /// </summary>
        /// <param name="playerID">The unique identifier of the player who's information should be obfuscated.</param>
        /// <param name="knownCards">Collection of cards known to the opposing player. These cards will not be obfuscated.</param>
        public void Obfuscate(int playerID, List<int> knownCards) {

            Controller obfuscatePlayer;
            if (playerID == Player1.Id) {
                obfuscatePlayer = Player1;
            }
            else if (playerID == Player2.Id) {
                obfuscatePlayer = Player2;
            }
            else return;

            // Hand
            foreach (var item in obfuscatePlayer.HandZone) {
                if (!knownCards.Contains(item.Id)) {
                    // TODO replace with a hidden card
                    obfuscatePlayer.HandZone.Replace(item, null);
                }
            }
            // Deck
            foreach (var item in obfuscatePlayer.DeckZone) {
                if (!knownCards.Contains(item.Id)) {
                    // TODO replace with a hidden card
                    obfuscatePlayer.DeckZone.Replace(item, null);
                }
            }
            // Secrets
            foreach (var item in obfuscatePlayer.SecretZone) {
                if (!knownCards.Contains(item.Id)) {
                    // TODO replace with a hidden card
                    obfuscatePlayer.SecretZone.Replace(item, null);
                }
            }

        }

        #endregion

        #region Overridden Methods

        /// <summary>
        /// Copies this SabberStoneState and returns it.
        /// </summary>
        /// <returns>New SabberStoneState that is a copy of this SabberStoneState.</returns>
        public override dynamic Copy() {
            return new SabberStoneState(Game.Clone());
        }

        /// <summary>
        /// Returns the unique identifier of the currently active player.
        /// </summary>
        /// <returns>The unique identifier of the currently active player.</returns>
        public override int CurrentPlayer() {
            return Game.CurrentPlayer.Id;
        }

        /// <summary>
        /// Returns the SabberStoneState's hash code.
        /// </summary>
        /// <returns>The SabberStoneState's hash code.</returns>
        public override long HashMethod() {
            return Game.Hash().GetHashCode();
        }

        /// <summary>
        /// Returns the number of players in the game.
        /// </summary>
        /// <returns>The number of players in the game.</returns>
        public override int NumberOfPlayers() {
            return 2;
        }

        #endregion

    }
}
