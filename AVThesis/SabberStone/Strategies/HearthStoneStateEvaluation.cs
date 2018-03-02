using System;
using AVThesis.Search;
using AVThesis.Search.Tree;

/// <summary>
/// Written by A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by G.J. Roelofs and T. Aliyev.
/// </summary>
namespace AVThesis.SabberStone.Strategies {

    /// <summary>
    /// Represents a way of evaluating a board state in HearthStone.
    /// </summary>
    public class HearthStoneStateEvaluation : IStateEvaluation<object, SabberStoneState, SabberStoneAction, object, SabberStoneAction, TreeSearchNode<SabberStoneState, SabberStoneAction>> {

        #region Fields

        private Random _r = new Random();

        #endregion

        #region Constructor

        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        public HearthStoneStateEvaluation() {
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
        public double Evaluate(SearchContext<object, SabberStoneState, SabberStoneAction, object, SabberStoneAction> context, TreeSearchNode<SabberStoneState, SabberStoneAction> node, SabberStoneState state) {
            //TODO implement Hearthstone state evaluation
            return _r.NextDouble();
        }

        #endregion

    }
}
