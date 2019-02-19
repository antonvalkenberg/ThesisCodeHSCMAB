using AVThesis.Agent;

/// <summary>
/// Written by A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by G.J. Roelofs and T. Aliyev.
/// </summary>
namespace AVThesis.Search {

    /// <summary>
    /// Defines what a strategy should have to play out a game from a certain position.
    /// </summary>
    /// <typeparam name="D"><see cref="SearchContext{D}"/></typeparam>
    /// <typeparam name="P"><see cref="SearchContext{P}"/></typeparam>
    /// <typeparam name="A"><see cref="SearchContext{A}"/></typeparam>
    /// <typeparam name="S"><see cref="SearchContext{S}"/></typeparam>
    /// <typeparam name="Sol"><see cref="SearchContext{Sol}"/></typeparam>
    public interface IPlayoutStrategy<D, P, A, S, Sol> where D : class where P : State where A : class where S : class where Sol : class {

        /// <summary>
        /// Plays a game to its end state. This end state should be determined by the goal strategy in the search's context.
        /// </summary>
        /// <param name="context">The context of the search.</param>
        /// <param name="position">The position from which to play out the game.</param>
        /// <returns>The end position.</returns>
        P Playout(SearchContext<D, P, A, S, Sol> context, P position);

    }

    /// <summary>
    /// A strategy that uses an Agent to produce an action for each consequent position until the goal position is met.
    /// </summary>
    /// <typeparam name="D"><see cref="SearchContext{D}"/></typeparam>
    /// <typeparam name="P"><see cref="SearchContext{P}"/></typeparam>
    /// <typeparam name="A"><see cref="SearchContext{A}"/></typeparam>
    /// <typeparam name="S"><see cref="SearchContext{S}"/></typeparam>
    /// <typeparam name="Sol"><see cref="SearchContext{Sol}"/></typeparam>
    public class AgentPlayout<D, P, A, S, Sol> : IPlayoutStrategy<D, P, A, S, Sol> where D : class where P : State where A : class where S : class where Sol : class {

        #region Fields

        private IAgent<SearchContext<D, P, A, S, Sol>, P, A> _agent;

        #endregion

        #region Properties

        /// <summary>
        /// The agent that produces actions to play out.
        /// </summary>
        public IAgent<SearchContext<D, P, A, S, Sol>, P, A> Agent { get => _agent; set => _agent = value; }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        /// <param name="agent">The agent.</param>
        public AgentPlayout(IAgent<SearchContext<D, P, A, S, Sol>, P, A> agent) {
            Agent = agent;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Plays a game to its end state. This end state should be determined by the goal strategy in the search's context.
        /// </summary>
        /// <param name="context">The context of the search.</param>
        /// <param name="position">The position from which to play out the game.</param>
        /// <returns>The end position.</returns>
        public P Playout(SearchContext<D, P, A, S, Sol> context, P position) {
            var goal = context.Goal;
            var actor = context.Application;

            // Ask the agent to play a move until we have reached the goal.
            while (!goal.Done(context, position)) {
                A action = Agent.Act(context, position);
                position = actor.Apply(context, position, action);
            }

            return position;
        }

        #endregion

    }

}
