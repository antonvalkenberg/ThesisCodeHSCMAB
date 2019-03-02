using System;
using System.Collections.Generic;
using System.Linq;
using AVThesis.Datastructures;
using AVThesis.Game;

/// <summary>
/// Written by A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by G.J. Roelofs and T. Aliyev.
/// </summary>
namespace AVThesis.Search.Tree.NMC {

    /// <summary>
    /// Naïve Monte Carlo Tree Search.
    /// </summary>
    /// <typeparam name="D"><see cref="SearchContext{D}"/></typeparam>
    /// <typeparam name="P"><see cref="SearchContext{P}"/></typeparam>
    /// <typeparam name="A"><see cref="SearchContext{A}"/></typeparam>
    /// <typeparam name="S"><see cref="SearchContext{S}"/></typeparam>
    /// <typeparam name="Sol"><see cref="SearchContext{Sol}"/></typeparam>
    public class NMCTS<D, P, A, S, Sol> : TreeSearch<D, P, A, S, Sol> where D : class where P : State where A : class, IMove where S : class where Sol : class {

        #region Helper Class

        private class LocalArm {
            public A Action { get; }
            private int Visits { get; set; }
            private double TotalReward { get; set; }
            public double ExpectedReward {
                get {
                    if (Visits <= 0) return 0;
                    return TotalReward / (Visits * 1.0);
                }
            }
            public LocalArm(A action) {
                Action = action;
            }
            public void Visit(double reward) {
                TotalReward += reward;
                Visits++;
            }
            public override string ToString() {
                return $"{Action} - v{Visits} - r{TotalReward}";
            }
        }

        #endregion

        #region Fields

        /// <summary>
        /// Random Number Generator.
        /// </summary>
        private readonly Random _rng = new Random();

        #endregion

        #region Properties

        /// <summary>
        /// A strategy used to determine whether to explore or exploit.
        /// </summary>
        public IExplorationStrategy<D, P, A, S, Sol> ExplorationStrategy { get; set; }

        /// <summary>
        /// A strategy used during the Simulation phase of NMCTS.
        /// </summary>
        public IPlayoutStrategy<D, P, A, S, Sol> PlayoutStrategy { get; set; }

        /// <summary>
        /// A strategy to sample actions during the Naïve Sampling process.
        /// </summary>
        public ISamplingStrategy<P, A> SamplingStrategy { get; set; }

        /// <summary>
        /// The policy for selecting an Action from the global Multi-Armed-Bandit.
        /// </summary>
        public double PolicyGlobal { get; set; }

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
        /// <param name="explorationStrategy">The exploration strategy.</param>
        /// <param name="solutionStrategy">The solution strategy.</param>
        /// <param name="samplingStrategy">The sampling strategy.</param>
        /// <param name="playoutStrategy">The playout strategy.</param>
        /// <param name="time">The amount of time allowed for the search.</param>
        /// <param name="iterations">The amount of iterations allowed for the search.</param>
        /// <param name="globalPolicy">The global policy.</param>
        public NMCTS(ITreeSelection<D, P, A, S, Sol> selectionStrategy,
            ITreeExpansion<D, P, A, S, Sol> expansionStrategy,
            ITreeBackPropagation<D, P, A, S, Sol> backPropagationStrategy,
            ITreeFinalNodeSelection<D, P, A, S, Sol> finalNodeSelectionStrategy,
            IStateEvaluation<D, P, A, S, Sol, TreeSearchNode<P, A>> evaluationStrategy,
            IExplorationStrategy<D, P, A, S, Sol> explorationStrategy,
            ISolutionStrategy<D, P, A, S, Sol, TreeSearchNode<P, A>> solutionStrategy,
            ISamplingStrategy<P, A> samplingStrategy,
            IPlayoutStrategy<D, P, A, S, Sol> playoutStrategy, long time, int iterations, double globalPolicy) :
            base(selectionStrategy, expansionStrategy, backPropagationStrategy, finalNodeSelectionStrategy,
                evaluationStrategy, solutionStrategy, time, iterations) {
            ExplorationStrategy = explorationStrategy;
            SamplingStrategy = samplingStrategy;
            PlayoutStrategy = playoutStrategy;
            PolicyGlobal = globalPolicy;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Creates a new builder.
        /// </summary>
        /// <returns>Builder.</returns>
        public static NMCTSBuilder<D, P, A, S, Sol> Builder() {
            return new NMCTSBuilder<D, P, A, S, Sol>();
        }

        /// <summary>
        /// Perform the search. Note: should set the Solution in the SearchContext and update its Status.
        /// </summary>
        /// <param name="context">The context within which the search happens.</param>
        public override void Search(SearchContext<D, P, A, S, Sol> context) {

            var clone = context.Cloner;
            var rootState = context.Source;

            DateTime endTime = DateTime.Now.AddMilliseconds(Time);
            int it = 0;

            // Setup for when we might be continuing a search from a specific node.
            TreeSearchNode<P, A> root = (TreeSearchNode<P, A>)context.StartNode;
            if (root == null) {
                root = new TreeSearchNode<P, A>(clone.Clone(rootState), null);
                context.StartNode = root;
            }

            // Set up a global MAB, to hold the value combinations created during the naïve-sampling process.
            var gMAB = new Dictionary<long, Dictionary<int, LocalArm>>();

            while ((Time == Constants.NO_LIMIT_ON_THINKING_TIME || DateTime.Now < endTime) && (Iterations == Constants.NO_LIMIT_ON_ITERATIONS || it < Iterations)) {

                it++;

                // SelectAndExpand
                var selectedNode = NaïveSelectAndExpand(context, root, gMAB);

                // Simulate
                P endState = PlayoutStrategy.Playout(context, selectedNode.State.Copy());

                // Backpropagation
                BackPropagationStrategy.BackPropagate(context, EvaluationStrategy, selectedNode, endState);
            }

            TreeSearchNode<P, A> finalNode = FinalNodeSelectionStrategy.SelectFinalNode(context, root);
            context.Solution = SolutionStrategy.Solution(context, finalNode);

            context.Status = SearchContext<D, P, A, S, Sol>.SearchStatus.Success;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Selects an Action by using the NaïveSampling method and expands the tree with this action if it is not already present.
        /// </summary>
        /// <param name="context">The current search context.</param>
        /// <param name="node">The node from which to expand the tree.</param>
        /// <param name="gMAB">The global Multi-Armed-Bandit collection.</param>
        /// <returns>A <see cref="TreeSearchNode{S,A}"/> from which represents the selected node for the Simulation phase.</returns>
        private TreeSearchNode<P, A> NaïveSelectAndExpand(SearchContext<D, P, A, S, Sol> context, TreeSearchNode<P, A> node, Dictionary<long, Dictionary<int, LocalArm>> gMAB) {
            var state = context.Cloner.Clone(node.State);
            var apply = context.Application;

            // a = NaïveSampling(node.state, player)
            // if `a' leads to a child of `node'
            // then
            //      return SelectAndExpand(node.GetChild(a))
            // else
            //      newNode = Apply(node.state, a)
            //      node.AddChild(newNode, a)
            //      return newNode

            // Find an action through the NaïveSampling process
            A action = NaïveSampling(context, state, gMAB);
            var actionHash = action.GetHashCode();

            // Check if any of the children of the current node have the sampled action as their payload
            var existingChild = node.Children.FirstOrDefault(i => i.Payload.GetHashCode() == actionHash);
            if (existingChild != null) {
                // Check if taking this action still has the same player as active
                if (existingChild.State.CurrentPlayer() == node.State.CurrentPlayer())
                    return NaïveSelectAndExpand(context, existingChild, gMAB);
                // If not, we have reached the end of t
                return existingChild;
            }

            // If none of the current children on the node have the action as payload, create a new child
            P newState = apply.Apply(context, state, action);
            var newNode = new TreeSearchNode<P, A>(node, newState, action);
            // Add it to the node's children and return the child
            node.AddChild(newNode);
            return newNode;
        }

        /// <summary>
        /// Uses the local Multi-Armed-Bandits to explore the action space and uses the global Multi-Armed-Bandit to exploit the best performing actions.
        /// </summary>
        /// <param name="context">The current search context.</param>
        /// <param name="state">The game state for the node.</param>
        /// <param name="gMAB">The global Multi-Armed-Bandit.</param>
        /// <returns>An <see cref="A"/> that was selected from the global Multi-Armed-Bandit.</returns>
        private A NaïveSampling(SearchContext<D, P, A, S, Sol> context, P state, Dictionary<long, Dictionary<int, LocalArm>> gMAB) {
            var apply = context.Application;
            var stateClone = context.Cloner.Clone(state);
            var stateHash = stateClone.HashMethod();
            if (!gMAB.ContainsKey(stateHash))
                gMAB.Add(stateHash, new Dictionary<int, LocalArm>());

            // Use a policy p_0 to determine whether to explore or exploit
            // If explore was selected
            //      x_1...x_n is sampled by using a policy p_l to select a value for each X_i in X independently.
            //      As a side effect, the resulting value combination is added to the global MAB.
            // If exploit was selected
            //      x_1...x_n is sampled by using a policy p_g to select a value combination using MAB_g.

            // Can only exploit if there is anything to exploit in the first place
            if (gMAB[stateHash].IsNullOrEmpty() || ExplorationStrategy.Policy(context, 0)) {
                // Explore
                
                // Create an action according to policy p_1
                A action = SamplingStrategy.Sample(stateClone);
                var actionHash = action.GetHashCode();
                // Evaluate the sampled action
                P endState = PlayoutStrategy.Playout(context, apply.Apply(context, stateClone, action));
                var tempNode = new TreeSearchNode<P, A> { Payload = action };
                double reward = EvaluationStrategy.Evaluate(context, tempNode, endState);
                // Add the action to the global MAB
                if (gMAB[stateHash].ContainsKey(actionHash))
                    gMAB[stateHash][actionHash].Visit(reward);
                else {
                    var newArm = new LocalArm(action);
                    newArm.Visit(reward);
                    gMAB[stateHash].Add(actionHash, newArm);
                }

                return action;
            }
            
            // Exploit; epsilon-greedy by returning the action with the highest expected reward with probability 1-e, otherwise returning random.
            return _rng.NextDouble() <= (1 - PolicyGlobal) ? gMAB[stateHash].Values.OrderByDescending(i => i.ExpectedReward).First().Action : gMAB[stateHash].RandomElementOrDefault().Value.Action;
        }

        #endregion
        
    }

}
