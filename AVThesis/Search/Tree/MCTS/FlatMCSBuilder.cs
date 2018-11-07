using AVThesis.Agent;
using AVThesis.Game;

/// <summary>
/// Written by A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by G.J. Roelofs and T. Aliyev.
/// </summary>
namespace AVThesis.Search.Tree.MCTS {

    public class FlatMCSBuilder<D, P, A, S, Sol> : TreeSearchBuilder<D, P, A, S, Sol> where D : class where P : State where A : class, IMove where S : class where Sol : class {
        
        #region Properties

        /// <summary>
        /// A strategy used during the Simulation phase of FlatMCS.
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
        public FlatMCSBuilder(IPlayoutStrategy<D, P, A, S, Sol> playoutStrategy, ISolutionStrategy<D, P, A, S, Sol, TreeSearchNode<P, A>> solutionStrategy) {
            PlayoutStrategy = playoutStrategy;
            SolutionStrategy = solutionStrategy;
        }

        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        public FlatMCSBuilder() {
            PlayoutStrategy = new AgentPlayout<D, P, A, S, Sol>(new RandomAgent<D, P, A, S, Sol>());
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Build a new Flat Monte Carlo Search.
        /// </summary>
        /// <returns>A new instance of <see cref="FlatMCS{D,P,A,S,Sol}"/>.</returns>
        public override ISearchStrategy<D, P, A, S, Sol> Build() {
            return new FlatMCS<D, P, A, S, Sol>(SelectionStrategy, ExpansionStrategy, BackPropagationStrategy, FinalNodeSelectionStrategy, EvaluationStrategy, SolutionStrategy, PlayoutStrategy, Time, Iterations);
        }

        #endregion

    }
}
