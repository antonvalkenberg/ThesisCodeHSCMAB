using SabberStoneCore.Model.Entities;

/// <summary>
/// Written by A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by G.J. Roelofs and T. Aliyev.
/// </summary>
namespace AVThesis.SabberStone {

    /// <summary>
    /// Defines what a bot should support to be able to play a game of Hearthstone through SabberStone.
    /// </summary>
    public interface ISabberStoneBot {
        
        /// <summary>
        /// Requests the bot to return a SabberStoneAction based on the current SabberStoneState.
        /// </summary>
        /// <param name="state">The current game state.</param>
        /// <returns>SabberStoneAction.</returns>
        SabberStoneAction Act(SabberStoneState state);

        /// <summary>
        /// Sets the Controller that the bot represents within a SabberStone Game.
        /// </summary>
        /// <param name="controller">This bot's Controller.</param>
        void SetController(Controller controller);

        /// <summary>
        /// Returns the player's ID.
        /// </summary>
        /// <returns>Integer representing the player's ID.</returns>
        int PlayerID();

        /// <summary>
        /// Returns the bot's name.
        /// </summary>
        /// <returns>String representing the bot's name.</returns>
        string Name();

    }
}
