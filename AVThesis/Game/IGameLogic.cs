using AVThesis.Datastructures;
using AVThesis.Search;

/// <summary>
/// Written by A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by G.J. Roelofs and T. Aliyev.
/// </summary>
namespace AVThesis.Game {

    /// <summary>
    /// A collection interface which defines the bare essentials required for Search in games.
    /// </summary>
    /// <typeparam name="D"><see cref="SearchContext{D}"/></typeparam>
    /// <typeparam name="P"><see cref="SearchContext{P}"/></typeparam>
    /// <typeparam name="A"><see cref="SearchContext{A}"/></typeparam>
    /// <typeparam name="S"><see cref="SearchContext{S}"/></typeparam>
    /// <typeparam name="Sol"><see cref="SearchContext{Sol}"/></typeparam>
    /// <typeparam name="T">A Type that is a sub-type of <typeparamref name="A"/> and represents the possibilities when expanding from a Position.</typeparam>
    public interface IGameLogic<D, P, A, S, Sol, out T> : IApplicationStrategy<D, P, A, S, Sol>, IExpansionStrategy<D, P, A, S, Sol, A>, IGoalStrategy<D, P, A, S, Sol> where D : class where P : State where A : class where S : class where Sol : class where T : A {

        /// <summary>
        /// Applies an Action to a Position which results in a new Position.
        /// </summary>
        /// <param name="context">The context of the search.</param>
        /// <param name="position">The Position to which the Action should be applied.</param>
        /// <param name="action">The Action to apply.</param>
        /// <returns>Position that is the result of applying the Action.</returns>
        new P Apply(SearchContext<D, P, A, S, Sol> context, P position, A action);

        /// <summary>
        /// Expand the search from a Position.
        /// </summary>
        /// <param name="context">The context of the search.</param>
        /// <param name="position">The position to expand from.</param>
        /// <returns>An enumeration of possible actions from the argument position.</returns>
        new IPositionGenerator<T> Expand(SearchContext<D, P, A, S, Sol> context, P position);

        /// <summary>
        /// Determines if a Position represents a completed search.
        /// </summary>
        /// <param name="context">The context of the search.</param>
        /// <param name="position">The Position.</param>
        /// <returns>Whether or not the search is done.</returns>
        new bool Done(SearchContext<D, P, A, S, Sol> context, P position);

        /// <summary>
        /// Calculate the scores of a Position.
        /// </summary>
        /// <param name="position">The Position.</param>
        /// <returns>Array of Double containing the scores per player, indexed by player ID.</returns>
        double[] Scores(P position);

    }
}
