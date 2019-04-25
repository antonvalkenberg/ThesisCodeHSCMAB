using System;
using AVThesis.Agent;
using AVThesis.Game;

/// <summary>
/// Copyright © 2019 Anton Valkenberg
/// Written by BSc. A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by MSc. G.-J. Roelofs, MSc. T. Aliyev and MSc. D. de Rydt.
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
        private static readonly double DEFAULT_C = 1 / Math.Sqrt(2);
        
        /// <summary>
        /// A default for the minimum amount of visits to a node before expansion is applied.
        /// </summary>
        private const int MINIMUM_VISITS_EXPANSION = 20;

        #endregion

        #region Properties

        /// <summary>
        /// A strategy to select a child node from a specific node. Default is <see cref="BestNodeSelection{D, P, A, S, Sol, N}"/>.
        /// </summary>
        public ITreeSelection<D, P, A, S, Sol> SelectionStrategy { get; set; } = new BestNodeSelection<D, P, A, S, Sol>(MINIMUM_VISITS_SELECTION, new ScoreUCB<P, A>(DEFAULT_C));

        /// <summary>
        /// A strategy to expand a node. Default is <see cref="MinimumTExpansion{D, P, A, S, Sol, N}"/>.
        /// </summary>
        public ITreeExpansion<D, P, A, S, Sol> ExpansionStrategy { get; set; } = new MinimumTExpansion<D, P, A, S, Sol>(MINIMUM_VISITS_EXPANSION);

        /// <summary>
        /// A strategy to play out the game from a state. Default is <see cref="RandomAgent{D, P, A, S, Sol}"/>.
        /// </summary>
        public IPlayoutStrategy<D, P, A, S, Sol> PlayoutStrategy { get; set; } = new AgentPlayout<D, P, A, S, Sol>(new RandomAgent<D, P, A, S, Sol>());

        /// <summary>
        /// A strategy to propagate the value of a simulation back up the tree. Default is <see cref="DefaultBackPropagation{D, P, A, S, Sol, N}"/>.
        /// </summary>
        public ITreeBackPropagation<D, P, A, S, Sol> BackPropagationStrategy { get; set; } = new DefaultBackPropagation<D, P, A, S, Sol>();

        /// <summary>
        /// A strategy to select the final node when the search ends. Default is <see cref="BestRatioFinalNodeSelection{D, P, A, S, Sol, N}"/>.
        /// </summary>
        public ITreeFinalNodeSelection<D, P, A, S, Sol> FinalNodeSelectionStrategy { get; set; } = new BestRatioFinalNodeSelection<D, P, A, S, Sol>();

        /// <summary>
        /// A strategy to evaluate the value of a state. Default is <see cref="WinLossDrawStateEvaluation{D, P, A, S, Sol, N}"/>.
        /// </summary>
        public IStateEvaluation<D, P, A, S, Sol, TreeSearchNode<P, A>> EvaluationStrategy { get; set; } = new WinLossDrawStateEvaluation<D, P, A, S, Sol, TreeSearchNode<P, A>>(1, -1, 0.0);

        /// <summary>
        /// The amount of time in milliseconds that the search is allowed to run for. Default is no limit.
        /// </summary>
        public long Time { get; set; } = Constants.NO_LIMIT_ON_THINKING_TIME;

        /// <summary>
        /// The amount of iterations that the search is allowed to use. Default is no limit.
        /// </summary>
        public int Iterations { get; set; } = Constants.NO_LIMIT_ON_ITERATIONS;

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
