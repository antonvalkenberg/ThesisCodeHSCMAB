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
        }

        #endregion

        #region Fields

        /// <summary>
        /// Random Number Generator.
        /// </summary>
        private Random _rng = new Random();

        #endregion

        #region Properties

        /// <summary>
        /// A strategy used during the Simulation phase of NMCTS.
        /// </summary>
        public IPlayoutStrategy<D, P, A, S, Sol> PlayoutStrategy { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public double PolicyExploreExploit { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public double PolicyLocal { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public double PolicyGlobal { get; set; }

        #endregion

        #region Constructors

        public NMCTS(ITreeSelection<D, P, A, S, Sol> selectionStrategy,
            ITreeExpansion<D, P, A, S, Sol> expansionStrategy,
            ITreeBackPropagation<D, P, A, S, Sol> backPropagationStrategy,
            ITreeFinalNodeSelection<D, P, A, S, Sol> finalNodeSelectionStrategy,
            IStateEvaluation<D, P, A, S, Sol, TreeSearchNode<P, A>> evaluationStrategy,
            ISolutionStrategy<D, P, A, S, Sol, TreeSearchNode<P, A>> solutionStrategy
            , IPlayoutStrategy<D, P, A, S, Sol> playoutStrategy, long time, int iterations, double exploreExploitPolicy, double localPolicy, double globalPolicy) :
            base(selectionStrategy, expansionStrategy, backPropagationStrategy, finalNodeSelectionStrategy,
                evaluationStrategy, solutionStrategy, time, iterations) {
            PlayoutStrategy = playoutStrategy;
            PolicyExploreExploit = exploreExploitPolicy;
            PolicyLocal = localPolicy;
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
            var gMAB = new Dictionary<P, List<LocalArm>>();

            while ((Time == Constants.NO_LIMIT_ON_THINKING_TIME || DateTime.Now < endTime) && (Iterations == Constants.NO_LIMIT_ON_ITERATIONS || it < Iterations)) {

                it++;

                // SelectAndExpand
                var selectedNode = NaïveSelectAndExpand(context, root, gMAB);

                // Simulate
                P endState = PlayoutStrategy.Playout(context, selectedNode.State);

                // Backpropagation
                BackPropagationStrategy.BackPropagate(context, EvaluationStrategy, selectedNode, endState);
            }

            TreeSearchNode<P, A> finalNode = FinalNodeSelectionStrategy.SelectFinalNode(context, root);
            context.Solution = SolutionStrategy.Solution(context, finalNode);

            context.Status = SearchContext<D, P, A, S, Sol>.SearchStatus.Success;
        }

        #endregion

        #region Private Methods

        private TreeSearchNode<P, A> NaïveSelectAndExpand(SearchContext<D, P, A, S, Sol> context, TreeSearchNode<P, A> node, Dictionary<P, List<LocalArm>> gMAB) {
            var clone = context.Cloner;
            var state = clone.Clone(node.State);
            var apply = context.Application;

            //      a = NaïveSampling(node.state, player)
            A action = NaïveSampling(context, node, state, gMAB);
            P newState = apply.Apply(context, state, action);

            //      if `a' leads to a child of `node'
            var childStates = node.Children.ToDictionary(i => i.State, i => i);
            if (childStates.ContainsKey(newState)) {
                //  then
                //      return SelectAndExpand(node.GetChild(a))
                return NaïveSelectAndExpand(context, childStates[newState], gMAB);
            }
            //      else
            //          newNode = Apply(node.state, a)
            //          node.AddChild(newNode, a)
            //          return newNode
            var newNode = new TreeSearchNode<P, A>(newState, action);
            node.AddChild(newNode);
            return newNode;
        }

        private A NaïveSampling(SearchContext<D, P, A, S, Sol> context, TreeSearchNode<P, A> node, P state, Dictionary<P, List<LocalArm>> gMAB) {
            var apply = context.Application;
            if (!gMAB.ContainsKey(state))
                gMAB.Add(state, new List<LocalArm>());

            // Use a policy p_0 to determine whether to explore or exploit
            // If explore was selected
            //      x_1...x_n is sampled by using a policy p_l to select a value for each X_i in X independently.
            //      As a side effect, the resulting value combination is added to the global MAB.
            // If exploit was selected
            //      x_1...x_n is sampled by using a policy p_g to select a value combination using MAB_g.

            // Can only exploit if there is anything to exploit in the first place
            if (gMAB[state].IsNullOrEmpty() || _rng.NextDouble() <= PolicyExploreExploit) {
                // Explore
                
                // Create an action according to policy p_1
                //TODO: create policy p_1 for NMCTS
                A action = null;
                // Evaluate the sampled action
                P newState = apply.Apply(context, state, action);
                double reward = EvaluationStrategy.Evaluate(context, node, newState);
                // Add the action to the global MAB
                if (gMAB[state].Any(i => i.Action.Equals(action)))
                    gMAB[state].First(i => i.Action.Equals(action)).Visit(reward);
                else {
                    var newArm = new LocalArm(action);
                    newArm.Visit(reward);
                    gMAB[state].Add(newArm);
                }

                return action;
            }
            
            // Exploit; epsilon-greedy by returning the action with the highest expected reward with probability 1-e, otherwise returning random.
            return _rng.NextDouble() > PolicyGlobal ? gMAB[state].OrderByDescending(i => i.ExpectedReward).First().Action : gMAB[state].RandomElementOrDefault().Action;
        }

        #endregion
        
    }

}
