using AVThesis.Datastructures;

/// <summary>
/// Written by A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by G.J. Roelofs and T. Aliyev.
/// </summary>
namespace AVThesis.Search {

    /// <summary>
    /// Defines the necessities for a strategy that deals with expanding the search space.
    /// </summary>
    /// <typeparam name="D"><see cref="SearchContext{D}"/></typeparam>
    /// <typeparam name="P"><see cref="SearchContext{P}"/></typeparam>
    /// <typeparam name="A"><see cref="SearchContext{A}"/></typeparam>
    /// <typeparam name="S"><see cref="SearchContext{S}"/></typeparam>
    /// <typeparam name="Sol"><see cref="SearchContext{Sol}"/></typeparam>
    /// <typeparam name="T">A Type that is a sub-type of <typeparamref name="A"/> and represents the possibilities when expanding from a Position.</typeparam>
    public interface IExpansionStrategy<D, P, A, S, Sol, out T> where D : class where P : State where A : class where S : class where Sol : class where T : A {

        /// <summary>
        /// Expand the search from a Position.
        /// </summary>
        /// <param name="context">The context of the search.</param>
        /// <param name="position">The position to expand from.</param>
        /// <returns>An enumeration of possible actions from the argument position.</returns>
        IPositionGenerator<T> Expand(SearchContext<D, P, A, S, Sol> context, P position);

    }

}
