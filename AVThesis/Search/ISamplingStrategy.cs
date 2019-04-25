/// <summary>
/// Copyright © 2019 Anton Valkenberg
/// Written by BSc. A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by MSc. G.-J. Roelofs, MSc. T. Aliyev and MSc. D. de Rydt.
/// </summary>
namespace AVThesis.Search {

    /// <summary>
    /// Defines what a sampling strategy should have.
    /// </summary>
    /// <typeparam name="P"><see cref="SearchContext{P}"/></typeparam>
    /// <typeparam name="A"><see cref="SearchContext{A}"/></typeparam>
    public interface ISamplingStrategy<in P, out A> where P : State where A : class {

        /// <summary>
        /// Returns a sample action.
        /// </summary>
        /// <param name="state">The state to sample an action for.</param>
        /// <returns>A sample action.</returns>
        A Sample(P state);

    }

}
