using System;
using AVThesis.Game;

/// <summary>
/// Copyright © 2019 Anton Valkenberg
/// Written by BSc. A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by MSc. G.-J. Roelofs, MSc. T. Aliyev and MSc. D. de Rydt.
/// </summary>
namespace AVThesis.Search.Tree.MCTS {

    /// <summary>
    /// Monte Carlo Tree Search.
    /// </summary>
    /// <typeparam name="D"><see cref="SearchContext{D}"/></typeparam>
    /// <typeparam name="P"><see cref="SearchContext{P}"/></typeparam>
    /// <typeparam name="A"><see cref="SearchContext{A}"/></typeparam>
    /// <typeparam name="S"><see cref="SearchContext{S}"/></typeparam>
    /// <typeparam name="Sol"><see cref="SearchContext{Sol}"/></typeparam>
    public class MCTS<D, P, A, S, Sol> : TreeSearch<D, P, A, S, Sol> where D : class where P : State where A : class, IMove where S : class where Sol : class {

        #region Properties

        /// <summary>
        /// A strategy used during the Simulation phase of MCTS.
        /// </summary>
        public IPlayoutStrategy<D, P, A, S, Sol> PlayoutStrategy { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        /// <param name="selectionStrategy">The selection strategy.</param>
        /// <param name="expansionStrategy">The expansion strategy.</param>
        /// <param name="backPropagationStrategy">The back propagation strategy.</param>
        /// <param name="finalNodeSelectionStrategy">The final node selection strategy.</param>
        /// <param name="evaluationStrategy">The state evaluation strategy.</param>
        /// <param name="solutionStrategy">The solution strategy.</param>
        /// <param name="playoutStrategy">The playout strategy.</param>
        /// <param name="time">The amount of time allowed for the search.</param>
        /// <param name="iterations">The amount of iterations allowed for the search.</param>
        public MCTS(ITreeSelection<D, P, A, S, Sol> selectionStrategy, ITreeExpansion<D, P, A, S, Sol> expansionStrategy, ITreeBackPropagation<D, P, A, S, Sol> backPropagationStrategy, ITreeFinalNodeSelection<D, P, A, S, Sol> finalNodeSelectionStrategy, IStateEvaluation<D, P, A, S, Sol, TreeSearchNode<P, A>> evaluationStrategy, ISolutionStrategy<D, P, A, S, Sol, TreeSearchNode<P, A>> solutionStrategy, IPlayoutStrategy<D, P, A, S, Sol> playoutStrategy, long time, int iterations) : base(selectionStrategy, expansionStrategy, backPropagationStrategy, finalNodeSelectionStrategy, evaluationStrategy, solutionStrategy, time, iterations) {
            PlayoutStrategy = playoutStrategy;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Creates a new builder.
        /// </summary>
        /// <returns>Builder.</returns>
        public static MCTSBuilder<D, P, A, S, Sol> Builder() {
            return new MCTSBuilder<D, P, A, S, Sol>();
        }

        /// <summary>
        /// Perform the search. Note: should set the Solution in the SearchContext and update its Status.
        /// </summary>
        /// <param name="context">The context within which the search happens.</param>
        public override void Search(SearchContext<D, P, A, S, Sol> context) {

            var clone = context.Cloner;
            var rootState = context.Source;
            var apply = context.Application;
            var goal = context.Goal;

            var endTime = DateTime.Now.AddMilliseconds(Time);
            var it = 0;

            // Setup for when we might be continuing a search from a specific node.
            var root = (TreeSearchNode<P, A>)context.StartNode;
            if (root == null) {
                root = new TreeSearchNode<P, A>(clone.Clone(rootState));
                context.StartNode = root;
            }

            while ((Time == Constants.NO_LIMIT_ON_THINKING_TIME || DateTime.Now < endTime) && (Iterations == Constants.NO_LIMIT_ON_ITERATIONS || it < Iterations)) {

                it++;

                var worldState = clone.Clone(rootState);

                // Selection
                bool done;
                var target = root;
                while(!(done = goal.Done(context, worldState)) && target.IsFullyExpanded()) {
                    target = SelectionStrategy.SelectNextNode(context, target);
                    worldState = apply.Apply(context, worldState, target.Payload);
                }

                // Expansion
                var endState = worldState;
                if (!done) {
                    var result = ExpansionStrategy.Expand(context, target, endState);
                    if (result != target) {
                        endState = apply.Apply(context, endState, result.Payload);
                        target = result;
                    }

                    // Simulation
                    endState = PlayoutStrategy.Playout(context, endState);
                }

                // Keep track of the maximum depth we reach
                var nodeDepth = target.CalculateDepth();
                if (nodeDepth > MaxDepth) MaxDepth = nodeDepth;

                // Backpropagation
                BackPropagationStrategy.BackPropagate(context, EvaluationStrategy, target, endState);
            }

            var finalNode = FinalNodeSelectionStrategy.SelectFinalNode(context, root);
            context.Solution = SolutionStrategy.Solution(context, finalNode);
            context.BudgetSpent = it;
            context.Status = SearchContext<D, P, A, S, Sol>.SearchStatus.Success;
        }

        #endregion

    }

}
