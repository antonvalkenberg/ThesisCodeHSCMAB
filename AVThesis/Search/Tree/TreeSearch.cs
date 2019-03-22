/// <summary>
/// Written by A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by G.J. Roelofs and T. Aliyev.
/// </summary>
namespace AVThesis.Search.Tree {

    /// <summary>
    /// Provides a base class from which a Tree Search strategy can be created.
    /// </summary>
    /// <typeparam name="D"><see cref="SearchContext{D}"/></typeparam>
    /// <typeparam name="P"><see cref="SearchContext{P}"/></typeparam>
    /// <typeparam name="A"><see cref="SearchContext{A}"/></typeparam>
    /// <typeparam name="S"><see cref="SearchContext{S}"/></typeparam>
    /// <typeparam name="Sol"><see cref="SearchContext{Sol}"/></typeparam>
    public abstract class TreeSearch<D, P, A, S, Sol> : ISearchStrategy<D, P, A, S, Sol> where D : class where P : State where A : class where S : class where Sol : class {

        #region Properties

        /// <summary>
        /// A strategy to select a child node from a specific node.
        /// </summary>
        public ITreeSelection<D, P, A, S, Sol> SelectionStrategy { get; set; }

        /// <summary>
        /// A strategy to expand a node.
        /// </summary>
        public ITreeExpansion<D, P, A, S, Sol> ExpansionStrategy { get; set; }

        /// <summary>
        /// A strategy to propagate the value of a simulation back up the tree.
        /// </summary>
        public ITreeBackPropagation<D, P, A, S, Sol> BackPropagationStrategy { get; set; }

        /// <summary>
        /// A strategy to select the final node when the search ends.
        /// </summary>
        public ITreeFinalNodeSelection<D, P, A, S, Sol> FinalNodeSelectionStrategy { get; set; }

        /// <summary>
        /// A strategy to evaluate the value of a state.
        /// </summary>
        public IStateEvaluation<D, P, A, S, Sol, TreeSearchNode<P, A>> EvaluationStrategy { get; set; }

        /// <summary>
        /// A strategy to construct a solution to the search after it has ended.
        /// </summary>
        public ISolutionStrategy<D, P, A, S, Sol, TreeSearchNode<P, A>> SolutionStrategy { get; set; }

        /// <summary>
        /// The amount of time in milliseconds that the search is allowed to run for.
        /// </summary>
        public long Time { get; set; }

        /// <summary>
        /// The amount of iterations that the search is allowed to use.
        /// </summary>
        public int Iterations { get; set; }

        /// <summary>
        /// The maximum depth of a node encountered during the search.
        /// </summary>
        public int MaxDepth { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs a new instance without time or iterative restrictions.
        /// </summary>
        /// <param name="selectionStrategy">The selection strategy.</param>
        /// <param name="expansionStrategy">The expansion strategy.</param>
        /// <param name="backPropagationStrategy">The back propagation strategy.</param>
        /// <param name="finalNodeSelectionStrategy">The final node selection strategy.</param>
        /// <param name="evaluationStrategy">The state evaluation strategy.</param>
        /// <param name="solutionStrategy">The solution strategy.</param>
        /// <param name="time">[Optional] The time budget for this search. Default value is <see cref="Constants.NO_LIMIT_ON_THINKING_TIME"/>.</param>
        /// <param name="iterations">[Optional] The iteration budget for this search. Default value is <see cref="Constants.NO_LIMIT_ON_ITERATIONS"/>.</param>
        protected TreeSearch(ITreeSelection<D, P, A, S, Sol> selectionStrategy, ITreeExpansion<D, P, A, S, Sol> expansionStrategy, ITreeBackPropagation<D, P, A, S, Sol> backPropagationStrategy, ITreeFinalNodeSelection<D, P, A, S, Sol> finalNodeSelectionStrategy, IStateEvaluation<D, P, A, S, Sol, TreeSearchNode<P, A>> evaluationStrategy, ISolutionStrategy<D, P, A, S, Sol, TreeSearchNode<P, A>> solutionStrategy, long time = Constants.NO_LIMIT_ON_THINKING_TIME, int iterations = Constants.NO_LIMIT_ON_ITERATIONS) {
            SelectionStrategy = selectionStrategy;
            ExpansionStrategy = expansionStrategy;
            BackPropagationStrategy = backPropagationStrategy;
            FinalNodeSelectionStrategy = finalNodeSelectionStrategy;
            EvaluationStrategy = evaluationStrategy;
            SolutionStrategy = solutionStrategy;
            Time = time;
            Iterations = iterations;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Perform the search. Note: should set the Solution in the SearchContext and update its Status.
        /// </summary>
        /// <param name="context">The context within which the search happens.</param>
        public abstract void Search(SearchContext<D, P, A, S, Sol> context);

        #endregion

    }
}
