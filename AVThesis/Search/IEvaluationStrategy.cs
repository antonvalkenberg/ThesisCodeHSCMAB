/// <summary>
/// Copyright © 2019 Anton Valkenberg
/// Written by BSc. A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by MSc. G.-J. Roelofs, MSc. T. Aliyev and MSc. D. de Rydt.
/// </summary>
namespace AVThesis.Search {

    /// <summary>
    /// Defines what an evaluation strategy needs to evaluate the cost of moving from one Position to another, or determining the value of a Position.
    /// </summary>
    /// <typeparam name="D"><see cref="SearchContext{D}"/></typeparam>
    /// <typeparam name="P"><see cref="SearchContext{P}"/></typeparam>
    /// <typeparam name="A"><see cref="SearchContext{A}"/></typeparam>
    /// <typeparam name="S"><see cref="SearchContext{S}"/></typeparam>
    /// <typeparam name="Sol"><see cref="SearchContext{Sol}"/></typeparam>
    public interface IEvaluationStrategy<D, P, A, S, Sol> where D : class where P : State where A : class where S : class where Sol : class {

        /// <summary>
        /// Determines the cost of moving from one Position to another via an Action.
        /// </summary>
        /// <param name="context">The context of the search.</param>
        /// <param name="from">The Position before the Action is applied.</param>
        /// <param name="action">The Action that is applied.</param>
        /// <param name="to">The Position after the Action is applied.</param>
        /// <returns>Double representing the cost of moving.</returns>
        double Cost(SearchContext<D, P, A, S, Sol> context, P from, A action, P to);

        /// <summary>
        /// Determines the value of a Position.
        /// </summary>
        /// <param name="context">The context of the search.</param>
        /// <param name="position">The Position to evaluate.</param>
        /// <returns>Double representing the value of the Position.</returns>
        double Evaluate(SearchContext<D, P, A, S, Sol> context, P position);
        
    }

}
