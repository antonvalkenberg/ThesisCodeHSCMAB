using AVThesis.Game;
using SabberStoneCore.Tasks;

/// <summary>
/// Written by A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by G.J. Roelofs and T. Aliyev.
/// </summary>
namespace AVThesis.SabberStone {

    /// <summary>
    /// Implementation of the IMove interface for using a PlayerTask from a SabberStone Game as an action in the search framework.
    /// </summary>
    public class SabberStoneAction : IMove {

        #region Fields

        private PlayerTask _action;

        #endregion

        #region Properties

        /// <summary>
        /// The PlayerTask that this SabberStoneAction represents.
        /// </summary>
        public PlayerTask Action { get => _action; set => _action = value; }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructs a new instance of a SabberStoneAction.
        /// </summary>
        /// <param name="action">The PlayerTaks that the SabberStoneAction represents.</param>
        public SabberStoneAction(PlayerTask action) {
            Action = action;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns the unique identifier of the player that this SabberStoneAction belongs to.
        /// </summary>
        /// <returns>Integer representing the unique identifier of the player that plays this SabberStoneAction.</returns>
        public int Player() {
            return Action.Controller.Id;
        }

        #endregion

    }
}
