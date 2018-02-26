using AVThesis.Datastructures;

/// <summary>
/// Written by A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by G.J. Roelofs and T. Aliyev.
/// </summary>
namespace AVThesis.Search {

    /// <summary>
    /// Defines what a strategy should have to determine if a SearchContext has reached its goal.
    /// </summary>
    /// <typeparam name="D"><see cref="SearchContext{D}"/></typeparam>
    /// <typeparam name="P"><see cref="SearchContext{P}"/></typeparam>
    /// <typeparam name="A"><see cref="SearchContext{A}"/></typeparam>
    /// <typeparam name="S"><see cref="SearchContext{S}"/></typeparam>
    /// <typeparam name="Sol"><see cref="SearchContext{Sol}"/></typeparam>
    public interface IGoalStrategy<D, P, A, S, Sol> where D : class where P : State where A : class where S : class where Sol : class {

        /// <summary>
        /// Determines if a Position represents a completed search.
        /// </summary>
        /// <param name="context">The context of the search.</param>
        /// <param name="position">The Position.</param>
        /// <returns>Whether or not the search is done.</returns>
        bool Done(SearchContext<D, P, A, S, Sol> context, P position);

    }
    
}
