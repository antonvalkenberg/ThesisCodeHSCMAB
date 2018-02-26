using System.Collections.Generic;
using AVThesis.Search;
using AVThesis.Datastructures;
using System.Linq;

/// <summary>
/// Written by A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by G.J. Roelofs and T. Aliyev.
/// </summary>
namespace AVThesis.Agent {

    /// <summary>
    /// An agent that randomly makes moves.
    /// </summary>
    /// <typeparam name="D"><see cref="SearchContext{D}"/></typeparam>
    /// <typeparam name="P"><see cref="SearchContext{P}"/></typeparam>
    /// <typeparam name="A"><see cref="SearchContext{A}"/></typeparam>
    /// <typeparam name="S"><see cref="SearchContext{S}"/></typeparam>
    /// <typeparam name="Sol"><see cref="SearchContext{Sol}"/></typeparam>
    public class RandomAgent<D, P, A, S, Sol> : IAgent<SearchContext<D, P, A, S, Sol>, P, A> where D : class where P : State where A : class where S : class where Sol : class {

        /// <summary>
        /// Returns a random action from all available action in the argument state.
        /// </summary>
        /// <param name="context">The context of the search.</param>
        /// <param name="state">The state to return an action for.</param>
        /// <returns>A random action from all available action in the argument state.</returns>
        public A Act(SearchContext<D, P, A, S, Sol> context, P state) {

            IEnumerable<A> things = context.Expansion.Expand(context, state);
            List<A> actions = things.ToList();

            return actions.RandomElementOrDefault();

        }

    }
}
