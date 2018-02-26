using AVThesis.Datastructures;

/// <summary>
/// Written by A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by G.J. Roelofs and T. Aliyev.
/// </summary>
namespace AVThesis.Search {

    /// <summary>
    /// Defines what is needed for a strategy that applies an Action to a Position, producing a new Position.
    /// </summary>
    /// <typeparam name="D"><see cref="SearchContext{D}"/></typeparam>
    /// <typeparam name="P"><see cref="SearchContext{P}"/></typeparam>
    /// <typeparam name="A"><see cref="SearchContext{A}"/></typeparam>
    /// <typeparam name="S"><see cref="SearchContext{S}"/></typeparam>
    /// <typeparam name="Sol"><see cref="SearchContext{Sol}"/></typeparam>
    public interface IApplicationStrategy<D, P, A, S, Sol> where D : class where P : State where A : class where S : class where Sol : class {

        /// <summary>
        /// Applies an Action to a Position which results in a new Position.
        /// </summary>
        /// <param name="context">The context of the search.</param>
        /// <param name="position">The Position to which the Action should be applied.</param>
        /// <param name="action">The Action to apply.</param>
        /// <returns>Position that is the result of apply the Action.</returns>
        P Apply(SearchContext<D, P, A, S, Sol> context, P position, A action);

    }

}
