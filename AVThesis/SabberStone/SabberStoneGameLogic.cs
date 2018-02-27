using System;
using AVThesis.Datastructures;
using AVThesis.Game;
using AVThesis.Search;

/// <summary>
/// Written by A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by G.J. Roelofs and T. Aliyev.
/// </summary>
namespace AVThesis.SabberStone {

    /// <summary>
    /// Handles the game specific logic required for search in SabberStone.
    /// </summary>
    public class SabberStoneGameLogic : IGameLogic<object, SabberStoneState, SabberStoneAction, object, SabberStoneAction, SabberStoneAction> {

        #region Constants

        private const double PLAYER_WIN_SCORE = 1.0;
        private const double PLAYER_LOSS_SCORE = 0.0;

        #endregion

        #region Constructor

        /// <summary>
        /// Constructs a new instance of SabberStoneGameLogic.
        /// </summary>
        public SabberStoneGameLogic() {
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Applies a SabberStoneAction to a SabberStoneState which results in a new SabberStoneState.
        /// </summary>
        /// <param name="context">The context of the search.</param>
        /// <param name="position">The state to which the action should be applied.</param>
        /// <param name="action">The action to apply.</param>
        /// <returns>SabberStoneState that is the result of applying the action.</returns>
        public SabberStoneState Apply(SearchContext<object, SabberStoneState, SabberStoneAction, object, SabberStoneAction> context, SabberStoneState position, SabberStoneAction action) {
            //TODO implement Apply for SabberStone
            throw new NotImplementedException();
        }

        /// <summary>
        /// Determines if a SabberStoneState represents a completed game.
        /// </summary>
        /// <param name="context">The context of the search.</param>
        /// <param name="position">The game state.</param>
        /// <returns>Whether or not the game is completed.</returns>
        public bool Done(SearchContext<object, SabberStoneState, SabberStoneAction, object, SabberStoneAction> context, SabberStoneState position) {
            return position.Game.State == SabberStoneCore.Enums.State.COMPLETE;
        }

        /// <summary>
        /// Expand the search from a SabberStoneState.
        /// </summary>
        /// <param name="context">The context of the search.</param>
        /// <param name="position">The state to expand from.</param>
        /// <returns>An enumeration of possible actions from the argument state.</returns>
        public IPositionGenerator<SabberStoneAction> Expand(SearchContext<object, SabberStoneState, SabberStoneAction, object, SabberStoneAction> context, SabberStoneState position) {
            //TODO implement Expand for SabberStone
            throw new NotImplementedException();
        }

        /// <summary>
        /// Calculate the scores of a SabberStoneState.
        /// </summary>
        /// <param name="position">The game state.</param>
        /// <returns>Array of Double containing the scores per player. Index 0 represents Player1's score.</returns>
        public double[] Scores(SabberStoneState position) {
            return new double[] {
                position.PlayerWon == position.Player1.Id ? PLAYER_WIN_SCORE : PLAYER_LOSS_SCORE,
                position.PlayerWon == position.Player2.Id ? PLAYER_WIN_SCORE : PLAYER_LOSS_SCORE
            };
        }

        #endregion

    }
}
