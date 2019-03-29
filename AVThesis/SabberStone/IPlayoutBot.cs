using System.Collections.Generic;
using AVThesis.Search;

/// <summary>
/// Written by A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by G.J. Roelofs and T. Aliyev.
/// </summary>
namespace AVThesis.SabberStone {

    /// <inheritdoc />
    /// <summary>
    /// Defines what a <see cref="ISabberStoneBot"/> should be able to do when being used as a playout bot.
    /// </summary>
    public interface IPlayoutBot : ISabberStoneBot {

        /// <summary>
        /// Process the completion of a playout.
        /// </summary>
        /// <param name="context">The current search context.</param>
        /// <param name="endState">The game state at the end of the playout.</param>
        void PlayoutCompleted(SearchContext<List<SabberStoneAction>, SabberStoneState, SabberStoneAction, object, SabberStoneAction> context, SabberStoneState endState);

    }

}
