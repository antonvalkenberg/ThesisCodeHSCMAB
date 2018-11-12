using System;
using System.Diagnostics;
using AVThesis.Agent;
using AVThesis.Game;
using AVThesis.Search;
using AVThesis.Search.Tree;
using AVThesis.Search.Tree.MCTS;

/// <summary>
/// Written by A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by G.J. Roelofs and T. Aliyev.
/// </summary>
namespace AVThesis.Test {

    public abstract class SearchTest<D, P, A, S> where D : class where P : State where A : class, IMove where S : class {

        #region Fields

        private P _state;
        private IGameLogic<D, P, A, S, A, A> _gameLogic;
        private IAgent<SearchContext<D, P, A, S, A>, P, A> _agent;

        #endregion

        #region Properties

        /// <summary>
        /// The state that is being tested.
        /// </summary>
        public P State { get => _state; set => _state = value; }
        /// <summary>
        /// The logic behind the game in which the search is being tested.
        /// </summary>
        public IGameLogic<D, P, A, S, A, A> GameLogic { get => _gameLogic; set => _gameLogic = value; }
        /// <summary>
        /// The agent that acts inside the game that is being tested.
        /// </summary>
        public IAgent<SearchContext<D, P, A, S, A>, P, A> Agent { get => _agent; set => _agent = value; }

        #endregion

        #region Public Methods

        public void TestFlatMCS() {
            // Search setup
            var builder = FlatMCS<D, P, A, S, A>.Builder();
            builder.EvaluationStrategy = new WinLossDrawStateEvaluation<D, P, A, S, A, TreeSearchNode<P, A>>(1, -10, 0);
            builder.Iterations = 10000;
            builder.PlayoutStrategy = new AgentPlayout<D, P, A, S, A>(Agent);
            builder.SelectionStrategy = new BestNodeSelection<D, P, A, S, A>(1000, new ScoreUCB<P, A>(1 / Math.Sqrt(2)));
            builder.SolutionStrategy = new ActionSolution<D, P, A, S, A, TreeSearchNode<P, A>>();

            // Test if the AI finds the correct solution.
            TestAI(SearchContext<D, P, A, S, A>.GameSearchSetup(GameLogic, null, State, null, builder.Build()));
        }

        public void TestMCTS() {
            // Search setup
            var builder = MCTS<D, P, A, S, A>.Builder();
            builder.ExpansionStrategy = new MinimumTExpansion<D, P, A, S, A>(5);
            builder.Iterations = 1000;
            builder.PlayoutStrategy = new AgentPlayout<D, P, A, S, A>(Agent);
            builder.SolutionStrategy = new ActionSolution<D, P, A, S, A, TreeSearchNode<P, A>>();

            // Test if the AI finds the correct solution.
            TestAI(SearchContext<D, P, A, S, A>.GameSearchSetup(GameLogic, null, State, null, builder.Build()));
        }

        /// <summary>
        /// Test an AI on the argument search context.
        /// </summary>
        /// <param name="context">The context of the search.</param>
        public abstract void TestAI(SearchContext<D, P, A, S, A> context);

        /// <summary>
        /// Plays the game until it is done, continously applying the solution to the search before starting a new search.
        /// </summary>
        /// <param name="context">The context of the search.</param>
        /// <returns>The state that satisfies the goal condition of the search.</returns>
        public P PlayGame(SearchContext<D, P, A, S, A> context) {

            P state = context.Source;

            while (!context.Goal.Done(context, context.Source)) {
                // Execute the search
                context.Execute();
                
                // Check if the search was successful
                if (context.Status != SearchContext<D, P, A, S, A>.SearchStatus.Success) {
                    throw new System.Exception("Search did not conclude successfully.");
                }

                // Apply the found solution
                state = context.Application.Apply(context, state, context.Solution);

                // Reset the context to start another search from the new state
                context.Reset();
                context.Source = state;

                Debug.WriteLine(state);
            }

            return state;
        }

        #endregion

    }
}
