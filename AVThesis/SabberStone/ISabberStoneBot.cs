using AVThesis.Search.Tree;
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

        /// <summary>
        /// Returns the amount of budget spent while calculating the latest action.
        /// Note: this amount is relative to the strategy that the bot uses, i.e. it can either represent time or iterations.
        /// </summary>
        /// <returns>The amount of budget spent while calculating the latest action.</returns>
        long BudgetSpent();

        /// <summary>
        /// Returns the maximum depth reached by the search technique.
        /// Note: only relevant for bots using a <see cref="TreeSearch{D,P,A,S,Sol}"/>
        /// </summary>
        /// <returns></returns>
        int MaxDepth();

    }
}
