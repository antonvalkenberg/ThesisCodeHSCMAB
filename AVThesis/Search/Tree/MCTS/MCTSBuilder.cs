using AVThesis.Game;

/// <summary>
/// Written by A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by G.J. Roelofs and T. Aliyev.
/// </summary>
namespace AVThesis.Search.Tree.MCTS {

    /// <summary>
    /// A Builder for Monte Carlo Tree Search.
    /// </summary>
    /// <typeparam name="D"><see cref="SearchContext{D}"/></typeparam>
    /// <typeparam name="P"><see cref="SearchContext{P}"/></typeparam>
    /// <typeparam name="A"><see cref="SearchContext{A}"/></typeparam>
    /// <typeparam name="S"><see cref="SearchContext{S}"/></typeparam>
    /// <typeparam name="Sol"><see cref="SearchContext{Sol}"/></typeparam>
    public class MCTSBuilder<D, P, A, S, Sol> : TreeSearchBuilder<D, P, A, S, Sol> where D : class where P : State where A : class, IMove where S : class where Sol : class {

        #region Properties

        /// <summary>
        /// A strategy for playing out a game during the Simulation phase of MCTS.
        /// </summary>
        public new IPlayoutStrategy<D, P, A, S, Sol> PlayoutStrategy { get; set; }

        /// <summary>
        /// A strategy for constructing a solution to the search.
        /// </summary>
        public ISolutionStrategy<D, P, A, S, Sol, TreeSearchNode<P, A>> SolutionStrategy { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        /// <param name="playoutStrategy">The playout strategy.</param>
        /// <param name="solutionStrategy">The solution strategy.</param>
        public MCTSBuilder(IPlayoutStrategy<D, P, A, S, Sol> playoutStrategy, ISolutionStrategy<D, P, A, S, Sol, TreeSearchNode<P, A>> solutionStrategy) {
            PlayoutStrategy = playoutStrategy;
            SolutionStrategy = solutionStrategy;
        }

        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        public MCTSBuilder() {
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Build a new Monte Carlo Tree Search.
        /// </summary>
        /// <returns>A new instance of <see cref="MCTS{D,P,A,S,Sol}"/>.</returns>
        public override ISearchStrategy<D, P, A, S, Sol> Build() {
            return new MCTS<D, P, A, S, Sol>(SelectionStrategy, ExpansionStrategy, BackPropagationStrategy, FinalNodeSelectionStrategy, EvaluationStrategy, SolutionStrategy, PlayoutStrategy, Time, Iterations);
        }

        #endregion

    }
}
