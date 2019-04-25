using AVThesis.Datastructures;

/// <summary>
/// Copyright © 2019 Anton Valkenberg
/// Written by BSc. A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by MSc. G.-J. Roelofs, MSc. T. Aliyev and MSc. D. de Rydt.
/// </summary>
namespace AVThesis.Search {

    /// <summary>
    /// Defines what a strategy should have to determine the solution to a search based on the final node.
    /// </summary>
    /// <typeparam name="D"><see cref="SearchContext{D}"/></typeparam>
    /// <typeparam name="P"><see cref="SearchContext{P}"/></typeparam>
    /// <typeparam name="A"><see cref="SearchContext{A}"/></typeparam>
    /// <typeparam name="S"><see cref="SearchContext{S}"/></typeparam>
    /// <typeparam name="Sol"><see cref="SearchContext{Sol}"/></typeparam>
    /// <typeparam name="N">A Type of node that the search uses.</typeparam>
    public interface ISolutionStrategy<D, P, A, S, Sol, in N> where D : class where P : State where A : class where S : class where Sol : class where N : Node<A> {

        /// <summary>
        /// Returns the solution to the search.
        /// </summary>
        /// <param name="context">The context of the search.</param>
        /// <param name="node">The final node of the search.</param>
        /// <returns>A solution to the search.</returns>
        Sol Solution(SearchContext<D, P, A, S, Sol> context, N node);
        
    }

    /// <summary>
    /// A solution strategy that returns the final node as the solution to the search.
    /// </summary>
    /// <typeparam name="D"><see cref="SearchContext{D}"/></typeparam>
    /// <typeparam name="P"><see cref="SearchContext{P}"/></typeparam>
    /// <typeparam name="A"><see cref="SearchContext{A}"/></typeparam>
    /// <typeparam name="S"><see cref="SearchContext{S}"/></typeparam>
    /// <typeparam name="Sol"><see cref="SearchContext{Sol}"/></typeparam>
    /// <typeparam name="N"><see cref="ISolutionStrategy{N}"/></typeparam>
    public class NodeSolution<D, P, A, S, Sol, N> : ISolutionStrategy<D, P, A, S, Sol, N> where D : class where P : State where A : class where S : class where Sol : N where N : Node<A> {

        /// <summary>
        /// Returns the solution to the search.
        /// </summary>
        /// <param name="context">The context of the search.</param>
        /// <param name="node">The final node of the search.</param>
        /// <returns>A solution to the search.</returns>
        public Sol Solution(SearchContext<D, P, A, S, Sol> context, N node) {
            return (Sol)node;
        }
        
    }

    /// <summary>
    /// A solution strategy that returns the final node's payload as the solution to the search.
    /// </summary>
    /// <typeparam name="D"><see cref="SearchContext{D}"/></typeparam>
    /// <typeparam name="P"><see cref="SearchContext{P}"/></typeparam>
    /// <typeparam name="A"><see cref="SearchContext{A}"/></typeparam>
    /// <typeparam name="S"><see cref="SearchContext{S}"/></typeparam>
    /// <typeparam name="Sol"><see cref="SearchContext{Sol}"/></typeparam>
    /// <typeparam name="N"><see cref="ISolutionStrategy{N}"/></typeparam>
    public class ActionSolution<D, P, A, S, Sol, N> : ISolutionStrategy<D, P, A, S, Sol, N> where D : class where P : State where A : class where S : class where Sol : class, A where N : Node<A> {

        /// <summary>
        /// Returns the solution to the search.
        /// </summary>
        /// <param name="context">The context of the search.</param>
        /// <param name="node">The final node of the search.</param>
        /// <returns>A solution to the search.</returns>
        public Sol Solution(SearchContext<D, P, A, S, Sol> context, N node) {
            return (Sol)node.Payload;
        }
        
    }

    /// <summary>
    /// A solution strategy that returns the final node's state as the solution to the search.
    /// </summary>
    /// <typeparam name="D"><see cref="SearchContext{D}"/></typeparam>
    /// <typeparam name="P"><see cref="SearchContext{P}"/></typeparam>
    /// <typeparam name="A"><see cref="SearchContext{A}"/></typeparam>
    /// <typeparam name="S"><see cref="SearchContext{S}"/></typeparam>
    /// <typeparam name="Sol"><see cref="SearchContext{Sol}"/></typeparam>
    /// <typeparam name="N"><see cref="ISolutionStrategy{N}"/></typeparam>
    public class StateSolution<D, P, A, S, Sol, N> : ISolutionStrategy<D, P, A, S, Sol, N> where D : class where P : State where A : class where S : class where Sol : P where N : SearchNode<P, A> {

        /// <summary>
        /// Returns the solution to the search.
        /// </summary>
        /// <param name="context">The context of the search.</param>
        /// <param name="node">The final node of the search.</param>
        /// <returns>A solution to the search.</returns>
        public Sol Solution(SearchContext<D, P, A, S, Sol> context, N node) {
            return (Sol)node.State;
        }
        
    }

}
