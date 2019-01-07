using System;
using System.Diagnostics;
using AVThesis.Agent;
using AVThesis.Game;
using AVThesis.Search;
using AVThesis.Search.LSI;
using AVThesis.Search.Tree;
using AVThesis.Search.Tree.MCTS;
using AVThesis.Search.Tree.NMC;

/// <summary>
/// Written by A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by G.J. Roelofs and T. Aliyev.
/// </summary>
namespace AVThesisTest {

    public abstract class SearchTest<D, P, A, S, SI> where D : class where P : State where A : class, IMove where S : class where SI : class {

        #region Properties

        /// <summary>
        /// The state that is being tested.
        /// </summary>
        public P State { get; set; }

        /// <summary>
        /// The logic behind the game in which the search is being tested.
        /// </summary>
        public IGameLogic<D, P, A, S, A, A> GameLogic { get; set; }

        /// <summary>
        /// The agent that acts inside the game that is being tested.
        /// </summary>
        public IAgent<SearchContext<D, P, A, S, A>, P, A> Agent { get; set; }

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
            builder.Iterations = 10000;
            builder.PlayoutStrategy = new AgentPlayout<D, P, A, S, A>(Agent);
            builder.SolutionStrategy = new ActionSolution<D, P, A, S, A, TreeSearchNode<P, A>>();

            // Test if the AI finds the correct solution.
            TestAI(SearchContext<D, P, A, S, A>.GameSearchSetup(GameLogic, null, State, null, builder.Build()));
        }

        public void TestNMCTS(ISamplingStrategy<P, A> samplingStrategy) {
            // Search setup
            var builder = NMCTS<D, P, A, S, A>.Builder();
            builder.ExplorationStrategy = new ChanceExploration<D, P, A, S, A>(0.5);
            builder.Iterations = 10000;
            builder.PlayoutStrategy = new AgentPlayout<D, P, A, S, A>(Agent);
            builder.PolicyGlobal = 0;
            builder.SamplingStrategy = samplingStrategy;
            builder.SolutionStrategy = new ActionSolution<D, P, A, S, A, TreeSearchNode<P, A>>();

            // Test if the AI finds the correct solution.
            TestAI(SearchContext<D, P, A, S, A>.GameSearchSetup(GameLogic, null, State, null, builder.Build()));
        }

        public void TestLSI(ISideInformationStrategy<D, P, A, S, A, SI> sideInformationStrategy, ILSISamplingStrategy<P, A, SI> samplingStrategy) {
            // Search setup
            var samplesForGeneration = 1000;
            var samplesForEvaluation = 1000;
            var playoutStrategy = new AgentPlayout<D, P, A, S, A>(Agent);
            var evaluationStrategy = new WinLossDrawStateEvaluation<D, P, A, S, A, TreeSearchNode<P, A>>(1, -10, 0);
            var search = new LSI<D, P, A, S, TreeSearchNode<P, A>, SI>(samplesForGeneration, samplesForEvaluation, sideInformationStrategy, samplingStrategy, playoutStrategy, evaluationStrategy, GameLogic);

            // Test if the AI finds the correct solution.
            var context = SearchContext<D, P, A, S, A>.Context(null, State, null, null, search, null);
            TestAI(context);
        }

        /// <summary>
        /// Test an AI on the argument search context.
        /// </summary>
        /// <param name="context">The context of the search.</param>
        public abstract void TestAI(SearchContext<D, P, A, S, A> context);

        /// <summary>
        /// Plays the game until it is done, continuously applying the solution to the search before starting a new search.
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
                    throw new Exception("Search did not conclude successfully.");
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
