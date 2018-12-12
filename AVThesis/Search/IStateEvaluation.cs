using AVThesis.Datastructures;
using AVThesis.Game;

/// <summary>
/// Written by A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by G.J. Roelofs and T. Aliyev.
/// </summary>
namespace AVThesis.Search {

    /// <summary>
    /// Defines ways to evaluate a state relative to a node.
    /// </summary>
    /// <typeparam name="D"><see cref="SearchContext{D}"/></typeparam>
    /// <typeparam name="P"><see cref="SearchContext{P}"/></typeparam>
    /// <typeparam name="A"><see cref="SearchContext{A}"/></typeparam>
    /// <typeparam name="S"><see cref="SearchContext{S}"/></typeparam>
    /// <typeparam name="Sol"><see cref="SearchContext{Sol}"/></typeparam>
    /// <typeparam name="N">A Type of node that the search uses.</typeparam>
    public interface IStateEvaluation<D, P, A, S, Sol, N> where D : class where P : State where A : class where S : class where Sol : class where N : Node<A> {

        /// <summary>
        /// Returns the value of the argument state with respect to the argument node.
        /// </summary>
        /// <param name="context">The context of the search.</param>
        /// <param name="node">The node that provides the context to evaluate the state.</param>
        /// <param name="state">The state that should be evaluated.</param>
        /// <returns>Double representing the value of the state with respect to the node.</returns>
        double Evaluate(SearchContext<D, P, A, S, Sol> context, N node, P state);
        
    }

    /// <summary>
    /// Evaluates a state by returning a constant value based on the winning player, from the perspective of the active player.
    /// </summary>
    /// <typeparam name="D"><see cref="SearchContext{D}"/></typeparam>
    /// <typeparam name="P"><see cref="SearchContext{P}"/></typeparam>
    /// <typeparam name="A"><see cref="SearchContext{A}"/></typeparam>
    /// <typeparam name="S"><see cref="SearchContext{S}"/></typeparam>
    /// <typeparam name="Sol"><see cref="SearchContext{Sol}"/></typeparam>
    /// <typeparam name="N"><see cref="IStateEvaluation{N}"/></typeparam>
    public class WinLossDrawStateEvaluation<D, P, A, S, Sol, N> : IStateEvaluation<D, P, A, S, Sol, N> where D : class where P : State where A : class, IMove where S : class where Sol : class where N : SearchNode<P, A> {

        #region Fields

        private double _win;
        private double _loss;
        private double _draw;

        #endregion

        #region Properties

        /// <summary>
        /// The value of a state in which the active player has won the game.
        /// </summary>
        public double Win { get => _win; set => _win = value; }
        /// <summary>
        /// The value of a state in which the active player has lost the game.
        /// </summary>
        public double Loss { get => _loss; set => _loss = value; }
        /// <summary>
        /// The value of a game that is a draw.
        /// </summary>
        public double Draw { get => _draw; set => _draw = value; }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        /// <param name="win">The value to return for a win.</param>
        /// <param name="loss">The value to return for a loss.</param>
        /// <param name="draw">The value to return for a draw.</param>
        public WinLossDrawStateEvaluation(double win, double loss, double draw) {
            Win = win;
            Loss = loss;
            Draw = draw;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns the value of the argument state with respect to the argument node.
        /// </summary>
        /// <param name="context">The context of the search.</param>
        /// <param name="node">The node that provides the context to evaluate the state.</param>
        /// <param name="state">The state that should be evaluated.</param>
        /// <returns>Double representing the value of the state with respect to the node.</returns>
        public double Evaluate(SearchContext<D, P, A, S, Sol> context, N node, P state) {
            int playerWon = state.PlayerWon;
            A move = node.Payload;

            if (move == null || playerWon == State.DRAW) {
                return Draw;
            } else {
                return move.Player() == playerWon ? Win : Loss;
            }
        }
        
        #endregion
    }

    /// <summary>
    /// Evaluates a state by using the evaluation strategy of the SearchContext, multiplied by a constant factor.
    /// </summary>
    /// <typeparam name="D"><see cref="SearchContext{D}"/></typeparam>
    /// <typeparam name="P"><see cref="SearchContext{P}"/></typeparam>
    /// <typeparam name="A"><see cref="SearchContext{A}"/></typeparam>
    /// <typeparam name="S"><see cref="SearchContext{S}"/></typeparam>
    /// <typeparam name="Sol"><see cref="SearchContext{Sol}"/></typeparam>
    /// <typeparam name="N"><see cref="IStateEvaluation{N}"/></typeparam>
    public class EvaluationStateEvaluation<D, P, A, S, Sol, N> : IStateEvaluation<D, P, A, S, Sol, N> where D : class where P : State where A : class, IMove where S : class where Sol : class where N : SearchNode<P, A> {

        #region Fields

        private double _k;

        #endregion

        #region Properties

        /// <summary>
        /// The factor with which to multiply the value that returns from the SearchContext's evaluation strategy.
        /// </summary>
        public double K { get => _k; set => _k = value; }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        /// <param name="k">The factor to multiply the evaluation value by.</param>
        public EvaluationStateEvaluation(double k = 1) {
            K = k;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns the value of the argument state with respect to the argument node.
        /// </summary>
        /// <param name="context">The context of the search.</param>
        /// <param name="node">The node that provides the context to evaluate the state.</param>
        /// <param name="state">The state that should be evaluated.</param>
        /// <returns>Double representing the value of the state with respect to the node.</returns>
        public double Evaluate(SearchContext<D, P, A, S, Sol> context, N node, P state) {
            A move = node.Payload;

            if (move == null) {
                return 0;
            } else {
                var evaluation = context.Evaluation;
                return K * evaluation.Cost(context, node.State, move, state);
            }
        }
        
        #endregion

    }

}
