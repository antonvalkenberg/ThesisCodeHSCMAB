/// <summary>
/// Copyright © 2019 Anton Valkenberg
/// Written by BSc. A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by MSc. G.-J. Roelofs, MSc. T. Aliyev and MSc. D. de Rydt.
/// </summary>
namespace AVThesis.Search.LSI {

    /// <summary>
    /// Defines what a strategy should have that tries to estimate the number of samples to be used in LSI.
    /// </summary>
    /// <typeparam name="D"><see cref="SearchContext{D}"/></typeparam>
    /// <typeparam name="P"><see cref="SearchContext{P}"/></typeparam>
    /// <typeparam name="A"><see cref="SearchContext{A}"/></typeparam>
    /// <typeparam name="S"><see cref="SearchContext{S}"/></typeparam>
    /// <typeparam name="Sol"><see cref="SearchContext{Sol}"/></typeparam>
    public interface IBudgetEstimationStrategy<D, P, A, S, Sol> where D : class where P : State where A : class where S : class where Sol : class {

        /// <summary>
        /// Determines the sample sizes to be used for the search.
        /// <param name="context">The current search context.</param>
        /// <param name="generationSamples">Reference to the amount of samples that will be used in the Generation phase.</param>
        /// <param name="evaluationSamples">Reference to the amount of samples that will be used in the Evaluation phase.</param>
        /// </summary>
        void DetermineSampleSizes(SearchContext<D, P, A, S, Sol> context, out int generationSamples, out int evaluationSamples);

    }

}
