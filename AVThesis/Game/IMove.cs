/// <summary>
/// Written by A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by G.J. Roelofs and T. Aliyev.
/// </summary>
namespace AVThesis.Game {

    /// <summary>
    /// Represents an in-game move made by a player.
    /// </summary>
    public interface IMove {

        /// <summary>
        /// Returns the unique identifier of the player that this move belongs to.
        /// </summary>
        /// <returns>Integer representing the unique identifier of the player that plays this move.</returns>
        int Player();

    }

}
