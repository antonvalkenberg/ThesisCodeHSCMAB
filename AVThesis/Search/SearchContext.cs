using AVThesis.Game;
// ReSharper disable IdentifierTypo

/// <summary>
/// Written by A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by G.J. Roelofs and T. Aliyev.
/// </summary>
namespace AVThesis.Search {

    /// <summary>
    /// A wrapper for an assortment of strategies used during a search.
    /// </summary>
    /// <typeparam name="D">A Type of domain within which the search is executed (e.g. a board, map or game-setup).</typeparam>
    /// <typeparam name="P">A Type of position within the domain (e.g. a board state, location on a map or game-state).</typeparam>
    /// <typeparam name="A">A Type of action that transforms a position into another position.</typeparam>
    /// <typeparam name="S">A Type of subject for which the search in conducted (e.g. an agent traversing a map).</typeparam>
    /// <typeparam name="Sol">A Type of solution that the search provides once finished.</typeparam>
    public class SearchContext<D, P, A, S, Sol> where D : class where P : State where A : class where S : class where Sol : class {

        #region Properties

        /// <summary>
        /// The search space.
        /// </summary>
        public D Domain { get; set; }

        /// <summary>
        /// The position in the search space from which the search begins.
        /// </summary>
        public P Source { get; set; }

        /// <summary>
        /// The position in that search space that is the goal of the search.
        /// </summary>
        public P Target { get; set; }

        /// <summary>
        /// The object or agent moving through the search space.
        /// </summary>
        public S Subject { get; set; }

        /// <summary>
        /// An object representing the solution to the search. Note: only valid if the Status of this SearchContext is <see cref="SearchStatus.Success"/>.
        /// </summary>
        public Sol Solution { get; set; }

        /// <summary>
        /// The search's current status. See <see cref="SearchStatus"/>.
        /// </summary>
        public SearchStatus Status { get; set; } = SearchStatus.Ready;

        /// <summary>
        /// The Node from which the search begins.
        /// </summary>
        public SearchNode<P, A> StartNode { get; set; }

        /// <summary>
        /// The strategy for searching within this Domain.
        /// </summary>
        public ISearchStrategy<D, P, A, S, Sol> Search { get; set; }

        /// <summary>
        /// The strategy for Node expansion during the search.
        /// </summary>
        public IExpansionStrategy<D, P, A, S, Sol, A> Expansion { get; set; }

        /// <summary>
        /// The strategy for transforming a position into another position through an action.
        /// </summary>
        public IApplicationStrategy<D, P, A, S, Sol> Application { get; set; }

        /// <summary>
        /// The strategy for evaluating the cost of moving from a position to another through an action, or a position's value.
        /// </summary>
        public IEvaluationStrategy<D, P, A, S, Sol> Evaluation { get; set; }

        /// <summary>
        /// The strategy for determining whether or not the search has reached its goal.
        /// </summary>
        public IGoalStrategy<D, P, A, S, Sol> Goal { get; set; }

        /// <summary>
        /// The strategy for cloning. Uses <see cref="StateClone{T}"/> by default.
        /// </summary>
        public ICloneStrategy<P> Cloner { get; set; } = new StateClone<P>();

        #endregion

        /// <summary>
        /// Enumeration indicating the status of the SearchContext.
        /// </summary>
        public enum SearchStatus {

            /// <summary>
            /// The SearchContext is ready for a search to be executed.
            /// </summary>
            Ready = 0,
            /// <summary>
            /// The SearchContext is currently in progress of searching.
            /// </summary>
            InProgress = 1,
            /// <summary>
            /// The SearchContext has successfully completed the search.
            /// </summary>
            Success = 2,
            /// <summary>
            /// The SearchContext has encountered a failure during the search.
            /// </summary>
            Failure = 3
        }

        #region Constructors

        /// <summary>
        /// Empty constructor.
        /// </summary>
        public SearchContext() {
        }

        /// <summary>
        /// Constructs a new SearchContext.
        /// </summary>
        /// <param name="domain"><see cref="SearchContext{D, P, A, S, Sol, N}.Domain"/></param>
        /// <param name="source"><see cref="SearchContext{D, P, A, S, Sol, N}.Source"/></param>
        /// <param name="target"><see cref="SearchContext{D, P, A, S, Sol, N}.Target"/></param>
        /// <param name="subject"><see cref="SearchContext{D, P, A, S, Sol, N}.Subject"/></param>
        /// <param name="solution"><see cref="SearchContext{D, P, A, S, Sol, N}.Solution"/></param>
        /// <param name="status"><see cref="SearchContext{D, P, A, S, Sol, N}.Status"/></param>
        /// <param name="startNode"><see cref="SearchContext{D, P, A, S, Sol, N}.StartNode"/></param>
        /// <param name="search"><see cref="SearchContext{D, P, A, S, Sol, N}.Search"/></param>
        /// <param name="expansion"><see cref="SearchContext{D, P, A, S, Sol, N}.Expansion"/></param>
        /// <param name="application"><see cref="SearchContext{D, P, A, S, Sol, N}.Application"/></param>
        /// <param name="evaluation"><see cref="SearchContext{D, P, A, S, Sol, N}.Evaluation"/></param>
        /// <param name="goal"><see cref="SearchContext{D, P, A, S, Sol, N}.Goal"/></param>
        /// <param name="cloner"><see cref="SearchContext{D, P, A, S, Sol, N}.Cloner"/></param>
        public SearchContext(D domain, P source, P target, S subject, Sol solution, SearchStatus status, SearchNode<P, A> startNode, ISearchStrategy<D, P, A, S, Sol> search, IExpansionStrategy<D, P, A, S, Sol, A> expansion, IApplicationStrategy<D, P, A, S, Sol> application, IEvaluationStrategy<D, P, A, S, Sol> evaluation, IGoalStrategy<D, P, A, S, Sol> goal, ICloneStrategy<P> cloner) {
            Domain = domain;
            Source = source?.Copy();
            Target = target?.Copy();
            Subject = subject;
            Solution = solution;
            Status = status;
            StartNode = startNode?.Copy();
            Search = search;
            Expansion = expansion;
            Application = application;
            Evaluation = evaluation;
            Goal = goal;
            Cloner = cloner;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Reset this context, ready to start another search.
        /// </summary>
        /// <param name="clearStartNode">[Optional] Whether or not to clear the search's start node. Default value is true.</param>
        /// <returns>This reset SearchContext.</returns>
        public SearchContext<D, P, A, S, Sol> Reset(bool clearStartNode = true) {
            if (clearStartNode) StartNode = null;
            Status = SearchStatus.Ready;
            Solution = null;

            return this;
        }

        /// <summary>
        /// Executes the search through the SearchStrategy.
        /// </summary>
        /// <returns>This SearchContext after the search has been completed.</returns>
        public SearchContext<D, P, A, S, Sol> Execute() {

            Status = SearchStatus.InProgress;

            Search.Search(this);

            return this;
        }

        /// <summary>
        /// Creates a new copy of this SearchContext by using the full constructor method.
        /// </summary>
        /// <returns>A new instance that is a copy of this SearchContext.</returns>
        public SearchContext<D, P, A, S, Sol> Copy() {
            return new SearchContext<D, P, A, S, Sol>(Domain, Source, Target, Subject, Solution, Status, StartNode, Search, Expansion, Application, Evaluation, Goal, Cloner);
        }

        /// <summary>
        /// Creates a SearchContext with nothing set but the programmatic defaults.
        /// </summary>
        /// <returns>A SearchContext with nothing set but the programmatic defaults.</returns>
        public static SearchContext<D, P, A, S, Sol> Context() {
            return new SearchContext<D, P, A, S, Sol>();
        }

        /// <summary>
        /// Creates a SearchContext that uses the provided search and expansion strategies.
        /// </summary>
        /// <param name="domain">The search space.</param>
        /// <param name="source">The position to start the search in.</param>
        /// <param name="target">The position that needs to be reached.</param>
        /// <param name="subject">An object or agent moving within the search space.</param>
        /// <param name="search">The search strategy to be used.</param>
        /// <param name="expand">The expansion strategy to be used.</param>
        /// <returns>A SearchContext with the provided arguments set.</returns>
        public static SearchContext<D, P, A, S, Sol> Context(D domain, P source, P target, S subject, ISearchStrategy<D, P, A, S, Sol> search, IExpansionStrategy<D, P, A, S, Sol, A> expand) {
            return new SearchContext<D, P, A, S, Sol> {
                Domain = domain,
                Source = source,
                Target = target,
                Subject = subject,
                Search = search,
                Expansion = expand
            };
        }

        /// <summary>
        /// Creates a SearchContext that uses the provided GameLogic as Application, Expansion and Goal strategies.
        /// </summary>
        /// <param name="gameLogic">The GameLogic that should be used.</param>
        /// <param name="domain"><see cref="SearchContext{D, P, A, S, Sol, N}.Domain"/></param>
        /// <param name="source"><see cref="SearchContext{D, P, A, S, Sol, N}.Source"/></param>
        /// <param name="subject"><see cref="SearchContext{D, P, A, S, Sol, N}.Subject"/></param>
        /// <param name="search"><see cref="SearchContext{D, P, A, S, Sol, N}.Search"/></param>
        /// <returns>A SearchContext with the provided arguments set.</returns>
        public static SearchContext<D, P, A, S, Sol> GameSearchSetup(IGameLogic<D, P, A, S, Sol, A> gameLogic, D domain, P source, S subject, ISearchStrategy<D, P, A, S, Sol> search) {
            var context = Context(domain, source, null, subject, search, null);
            context.Application = gameLogic;
            context.Expansion = gameLogic;
            context.Goal = gameLogic;
            return context;
        }

        #endregion

    }

}
