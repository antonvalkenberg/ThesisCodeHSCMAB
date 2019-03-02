using System;
using System.Diagnostics;
using AVThesis.Agent;
using AVThesis.Game;
using AVThesis.Search;
using AVThesis.Search.LSI;
using AVThesis.Search.Tree;

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

        public void TestFlatMCS(ISearchStrategy<D, P, A, S, A> search) {
            TestAI(SearchContext<D, P, A, S, A>.GameSearchSetup(GameLogic, null, State, null, search));
        }

        public void TestMCTS(ISearchStrategy<D, P, A, S, A> search) {
            TestAI(SearchContext<D, P, A, S, A>.GameSearchSetup(GameLogic, null, State, null, search));
        }

        public void TestNMCTS(ISearchStrategy<D, P, A, S, A> search) {
            TestAI(SearchContext<D, P, A, S, A>.GameSearchSetup(GameLogic, null, State, null, search));
        }

        public void TestLSI(LSI<D, P, A, S, TreeSearchNode<P, A>, SI> search) {
            TestAI(SearchContext<D, P, A, S, A>.GameSearchSetup(GameLogic, null, State, null, search));
        }

        public abstract ISearchStrategy<D, P, A, S, A> SetupFlatMCS();

        public abstract ISearchStrategy<D, P, A, S, A> SetupMCTS();

        public abstract ISearchStrategy<D, P, A, S, A> SetupNMCTS(ISamplingStrategy<P, A> samplingStrategy);

        public abstract LSI<D, P, A, S, TreeSearchNode<P, A>, SI> SetupLSI(ISideInformationStrategy<D, P, A, S, A, SI> sideInformationStrategy, ILSISamplingStrategy<P, A, SI> samplingStrategy);

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

            var state = context.Source;

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
