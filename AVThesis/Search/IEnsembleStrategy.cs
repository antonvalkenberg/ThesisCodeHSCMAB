/// <summary>
/// Written by A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by G.J. Roelofs and T. Aliyev.
/// </summary>
namespace AVThesis.Search {

    /// <summary>
    /// Defines what an ensemble-strategy should have when searching with a SearchContext.
    /// </summary>
    /// <typeparam name="D"><see cref="SearchContext{D}"/></typeparam>
    /// <typeparam name="P"><see cref="SearchContext{P}"/></typeparam>
    /// <typeparam name="A"><see cref="SearchContext{A}"/></typeparam>
    /// <typeparam name="S"><see cref="SearchContext{S}"/></typeparam>
    /// <typeparam name="Sol"><see cref="SearchContext{Sol}"/></typeparam>
    public interface IEnsembleStrategy<D, P, A, S, Sol> where D : class where P : State where A : class where S : class where Sol : class {

        /// <summary>
        /// Perform the search. Note: should set the Solution in the SearchContext and update its Status.
        /// </summary>
        /// <param name="context">The context within which the search happens.</param>
        /// <param name="ensembleSize">The size of the ensemble.</param>
        void EnsembleSearch(SearchContext<D, P, A, S, Sol> context, int ensembleSize);

        /// <summary>
        /// Returns the solution to the search.
        /// </summary>
        /// <param name="context">The context of the search.</param>
        /// <returns>A solution to the search.</returns>
        Sol Solution(SearchContext<D, P, A, S, Sol> context);

    }

}
