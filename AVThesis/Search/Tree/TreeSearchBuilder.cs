using System;
using AVThesis.Agent;
using AVThesis.Game;

/// <summary>
/// Written by A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by G.J. Roelofs and T. Aliyev.
/// </summary>
namespace AVThesis.Search.Tree {

    /// <summary>
    /// Defines the builder components needed to create a tree search strategy for searching in games.
    /// </summary>
    /// <typeparam name="D"><see cref="SearchContext{D}"/></typeparam>
    /// <typeparam name="P"><see cref="SearchContext{P}"/></typeparam>
    /// <typeparam name="A"><see cref="SearchContext{A}"/></typeparam>
    /// <typeparam name="S"><see cref="SearchContext{S}"/></typeparam>
    /// <typeparam name="Sol"><see cref="SearchContext{Sol}"/></typeparam>
    public abstract class TreeSearchBuilder<D, P, A, S, Sol> : ISearchBuilder<D, P, A, S, Sol> where D : class where P : State where A : class, IMove where S : class where Sol : class {

        #region Constants
        
        /// <summary>
        /// A default for the minimum amount of visits to a node before evaluation should be used during node selection.
        /// </summary>
        private const int MINIMUM_VISITS_SELECTION = 20;
        /// <summary>
        /// A default value for the C-constant used in UCB (value taken from literature).
        /// </summary>
        private static double DEFAULT_C = 1 / Math.Sqrt(2);
        /// <summary>
        /// A default for the minimum amount of visits to a node before expansion is applied.
        /// </summary>
        private const int MINIMUM_VISITS_EXPANSION = 20;

        #endregion

        #region Fields

        private ITreeSelection<D, P, A, S, Sol> _selectionStrategy = new BestNodeSelection<D, P, A, S, Sol>(MINIMUM_VISITS_SELECTION, new ScoreUCB<P, A>(DEFAULT_C));
        private ITreeExpansion<D, P, A, S, Sol> _expansionStrategy = new MinimumTExpansion<D, P, A, S, Sol>(MINIMUM_VISITS_EXPANSION);
        private IPlayoutStrategy<D, P, A, S, Sol> _playoutStrategy = new AgentPlayout<D, P, A, S, Sol>(new RandomAgent<D, P, A, S, Sol>());
        private ITreeBackPropagation<D, P, A, S, Sol> _backPropagationStrategy = new DefaultBackPropagation<D, P, A, S, Sol>();
        private ITreeFinalNodeSelection<D, P, A, S, Sol> _finalNodeSelectionStrategy = new BestRatioFinalNodeSelection<D, P, A, S, Sol>();
        private IStateEvaluation<D, P, A, S, Sol, TreeSearchNode<P, A>> _evaluationStrategy = new WinLossDrawStateEvaluation<D, P, A, S, Sol, TreeSearchNode<P, A>>(1, -1, 0.0);
        private long _time = Constants.NO_LIMIT_ON_THINKING_TIME;
        private int _iterations = Constants.NO_LIMIT_ON_ITERATIONS;

        #endregion

        #region Properties

        /// <summary>
        /// A strategy to select a child node from a specific node. Default is <see cref="BestNodeSelection{D, P, A, S, Sol, N}"/>.
        /// </summary>
        public ITreeSelection<D, P, A, S, Sol> SelectionStrategy { get => _selectionStrategy; set => _selectionStrategy = value; }
        /// <summary>
        /// A strategy to expand a node. Default is <see cref="MinimumTExpansion{D, P, A, S, Sol, N}"/>.
        /// </summary>
        public ITreeExpansion<D, P, A, S, Sol> ExpansionStrategy { get => _expansionStrategy; set => _expansionStrategy = value; }
        /// <summary>
        /// A strategy to play out the game from a state. Default is <see cref="RandomAgent{D, P, A, S, Sol}"/>.
        /// </summary>
        public IPlayoutStrategy<D, P, A, S, Sol> PlayoutStrategy { get => _playoutStrategy; set => _playoutStrategy = value; }
        /// <summary>
        /// A strategy to propagate the value of a simulation back up the tree. Default is <see cref="DefaultBackPropagation{D, P, A, S, Sol, N}"/>.
        /// </summary>
        public ITreeBackPropagation<D, P, A, S, Sol> BackPropagationStrategy { get => _backPropagationStrategy; set => _backPropagationStrategy = value; }
        /// <summary>
        /// A strategy to select the final node when the search ends. Default is <see cref="BestRatioFinalNodeSelection{D, P, A, S, Sol, N}"/>.
        /// </summary>
        public ITreeFinalNodeSelection<D, P, A, S, Sol> FinalNodeSelectionStrategy { get => _finalNodeSelectionStrategy; set => _finalNodeSelectionStrategy = value; }
        /// <summary>
        /// A strategy to evaluate the value of a state. Default is <see cref="WinLossDrawStateEvaluation{D, P, A, S, Sol, N}"/>.
        /// </summary>
        public IStateEvaluation<D, P, A, S, Sol, TreeSearchNode<P, A>> EvaluationStrategy { get => _evaluationStrategy; set => _evaluationStrategy = value; }
        /// <summary>
        /// The amount of time in milliseconds that the search is allowed to run for. Default is no limit.
        /// </summary>
        public long Time { get => _time; set => _time = value; }
        /// <summary>
        /// The amount of iterations that the search is allowed to use. Default is no limit.
        /// </summary>
        public int Iterations { get => _iterations; set => _iterations = value; }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        public TreeSearchBuilder() {
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Build the tree search strategy.
        /// </summary>
        /// <returns>The created TreeSearch.</returns>
        public abstract ISearchStrategy<D, P, A, S, Sol> Build();

        #endregion
    }
}
