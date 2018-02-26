using AVThesis.Datastructures;
using AVThesis.Game;

/// <summary>
/// Written by A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by G.J. Roelofs and T. Aliyev.
/// </summary>
namespace AVThesis.Search {

    /// <summary>
    /// Defines the builder components needed to create a search strategy for searching in games.
    /// </summary>
    /// <typeparam name="D"><see cref="SearchContext{D}"/></typeparam>
    /// <typeparam name="P"><see cref="SearchContext{P}"/></typeparam>
    /// <typeparam name="A"><see cref="SearchContext{A}"/></typeparam>
    /// <typeparam name="S"><see cref="SearchContext{S}"/></typeparam>
    /// <typeparam name="Sol"><see cref="SearchContext{Sol}"/></typeparam>
    public interface ISearchBuilder<D, P, A, S, Sol> where D : class where P : State where A : class, IMove where S : class where Sol : class {

        /// <summary>
        /// Builds the search strategy.
        /// </summary>
        /// <returns>A SearchStrategy.</returns>
        ISearchStrategy<D, P, A, S, Sol> Build();

    }
}
