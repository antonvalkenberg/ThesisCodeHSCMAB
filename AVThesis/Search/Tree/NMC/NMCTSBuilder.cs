using AVThesis.Agent;
using AVThesis.Game;

/// <summary>
/// Written by A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by G.J. Roelofs and T. Aliyev.
/// </summary>
namespace AVThesis.Search.Tree.NMC {

    /// <summary>
    /// A Builder for Naïve Monte Carlo Tree Search.
    /// </summary>
    /// <typeparam name="D"><see cref="SearchContext{D}"/></typeparam>
    /// <typeparam name="P"><see cref="SearchContext{P}"/></typeparam>
    /// <typeparam name="A"><see cref="SearchContext{A}"/></typeparam>
    /// <typeparam name="S"><see cref="SearchContext{S}"/></typeparam>
    /// <typeparam name="Sol"><see cref="SearchContext{Sol}"/></typeparam>
    public class NMCTSBuilder<D, P, A, S, Sol> : TreeSearchBuilder<D, P, A, S, Sol> where D : class where P : State where A : class, IMove where S : class where Sol : class {

        #region Properties

        /// <summary>
        /// A strategy used to determine whether to explore or exploit.
        /// </summary>
        public IExplorationStrategy<D, P, A, S, Sol> ExplorationStrategy { get; set; }

        /// <summary>
        /// A strategy used during the Simulation phase of NMCTS.
        /// </summary>
        public new IPlayoutStrategy<D, P, A, S, Sol> PlayoutStrategy { get; set; }

        /// <summary>
        /// A strategy to sample actions during the Naïve Sampling process.
        /// </summary>
        public ISamplingStrategy<D, P, A, S, Sol> SamplingStrategy { get; set; }

        /// <summary>
        /// A strategy for constructing a solution to the search.
        /// </summary>
        public ISolutionStrategy<D, P, A, S, Sol, TreeSearchNode<P, A>> SolutionStrategy { get; set; }

        /// <summary>
        /// The policy for selecting an Action from the global Multi-Armed-Bandit.
        /// </summary>
        public double PolicyGlobal { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        /// <param name="explorationStrategy">The exploration strategy.</param>
        /// <param name="playoutStrategy">The playout strategy.</param>
        /// <param name="samplingStrategy">The sampling strategy.</param>
        /// <param name="solutionStrategy">The solution strategy.</param>
        /// <param name="policyGlobal">The global policy.</param>
        public NMCTSBuilder(IExplorationStrategy<D, P, A, S, Sol> explorationStrategy, IPlayoutStrategy<D, P, A, S, Sol> playoutStrategy, ISamplingStrategy<D, P, A, S, Sol> samplingStrategy, ISolutionStrategy<D, P, A, S, Sol, TreeSearchNode<P, A>> solutionStrategy, double policyGlobal) {
            ExplorationStrategy = explorationStrategy;
            PlayoutStrategy = playoutStrategy;
            SamplingStrategy = samplingStrategy;
            SolutionStrategy = solutionStrategy;
            PolicyGlobal = policyGlobal;
        }

        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        public NMCTSBuilder() {
            ExplorationStrategy = new ChanceExploration<D, P, A, S, Sol>(Constants.DEFAULT_EXPLORE_CHANCE);
            PlayoutStrategy = new AgentPlayout<D, P, A, S, Sol>(new RandomAgent<D, P, A, S, Sol>());
            PolicyGlobal = 0; // pure-greedy, i.e. e-greedy with e=0
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Build a new Naïve Monte Carlo Tree Search.
        /// </summary>
        /// <returns>A new instance of <see cref="NMCTS{D, P, A, S, Sol}"/>.</returns>
        public override ISearchStrategy<D, P, A, S, Sol> Build() {
            return new NMCTS<D, P, A, S, Sol>(SelectionStrategy, ExpansionStrategy, BackPropagationStrategy, FinalNodeSelectionStrategy, EvaluationStrategy, ExplorationStrategy, SolutionStrategy, SamplingStrategy, PlayoutStrategy, Time, Iterations, PolicyGlobal);
        }

        #endregion
        
    }
}
