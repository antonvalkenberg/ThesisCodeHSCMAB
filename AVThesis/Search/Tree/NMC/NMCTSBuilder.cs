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
        /// A strategy used during the Simulation phase of NMCTS.
        /// </summary>
        public new IPlayoutStrategy<D, P, A, S, Sol> PlayoutStrategy { get; set; }

        /// <summary>
        /// A strategy for constructing a solution to the search.
        /// </summary>
        public ISolutionStrategy<D, P, A, S, Sol, TreeSearchNode<P, A>> SolutionStrategy { get; set; }

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

        public NMCTSBuilder(IPlayoutStrategy<D, P, A, S, Sol> playoutStrategy, ISolutionStrategy<D, P, A, S, Sol, TreeSearchNode<P, A>> solutionStrategy, double policyExploreExploit, double policyLocal, double policyGlobal) {
            PlayoutStrategy = playoutStrategy;
            SolutionStrategy = solutionStrategy;
            PolicyExploreExploit = policyExploreExploit;
            PolicyLocal = policyLocal;
            PolicyGlobal = policyGlobal;
        }

        public NMCTSBuilder() {
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Build a new Naïve Monte Carlo Tree Search.
        /// </summary>
        /// <returns>A new instance of <see cref="NMCTS{D, P, A, S, Sol}"/>.</returns>
        public override ISearchStrategy<D, P, A, S, Sol> Build() {
            return new NMCTS<D, P, A, S, Sol>(SelectionStrategy, ExpansionStrategy, BackPropagationStrategy, FinalNodeSelectionStrategy, EvaluationStrategy, SolutionStrategy, PlayoutStrategy, Time, Iterations, PolicyExploreExploit, PolicyLocal, PolicyGlobal);
        }

        #endregion


    }
}
