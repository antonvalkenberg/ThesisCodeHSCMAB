using AVThesis.Game;

/// <summary>
/// Copyright © 2019 Anton Valkenberg
/// Written by BSc. A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by MSc. G.-J. Roelofs, MSc. T. Aliyev and MSc. D. de Rydt.
/// </summary>
namespace AVThesis.Search.Tree {

    /// <summary>
    /// Defines what a strategy should do that handles the back propagation in a tree search.
    /// </summary>
    /// <typeparam name="D"><see cref="SearchContext{D}"/></typeparam>
    /// <typeparam name="P"><see cref="SearchContext{P}"/></typeparam>
    /// <typeparam name="A"><see cref="SearchContext{A}"/></typeparam>
    /// <typeparam name="S"><see cref="SearchContext{S}"/></typeparam>
    /// <typeparam name="Sol"><see cref="SearchContext{Sol}"/></typeparam>
    public interface ITreeBackPropagation<D, P, A, S, Sol> where D : class where P : State where A : class where S : class where Sol : class {

        /// <summary>
        /// Propagate an evaluation value of the argument state starting from the argument node back up to the root node.
        /// </summary>
        /// <param name="context">The context of the search.</param>
        /// <param name="evaluation">The strategy used to evaluate the state.</param>
        /// <param name="node">The node from which the backpropagation starts.</param>
        /// <param name="state">The state that should be evaluated.</param>
        void BackPropagate(SearchContext<D, P, A, S, Sol> context, IStateEvaluation<D, P, A, S, Sol, TreeSearchNode<P, A>> evaluation, TreeSearchNode<P, A> node, P state);

    }

    /// <summary>
    /// Default backpropagation strategy that visits each node up to the root node with the evaluation value of the state with respect to that node.
    /// </summary>
    /// <typeparam name="D"><see cref="SearchContext{D}"/></typeparam>
    /// <typeparam name="P"><see cref="SearchContext{P}"/></typeparam>
    /// <typeparam name="A"><see cref="SearchContext{A}"/></typeparam>
    /// <typeparam name="S"><see cref="SearchContext{S}"/></typeparam>
    /// <typeparam name="Sol"><see cref="SearchContext{Sol}"/></typeparam>
    public class DefaultBackPropagation<D, P, A, S, Sol> : ITreeBackPropagation<D, P, A, S, Sol> where D : class where P : State where A : class, IMove where S : class where Sol : class {

        /// <summary>
        /// Propagate an evaluation value of the argument state starting from the argument node back up to the root node.
        /// </summary>
        /// <param name="context">The context of the search.</param>
        /// <param name="evaluation">The strategy used to evaluate the state.</param>
        /// <param name="node">The node from which the backpropagation starts.</param>
        /// <param name="state">The state that should be evaluated.</param>
        public void BackPropagate(SearchContext<D, P, A, S, Sol> context, IStateEvaluation<D, P, A, S, Sol, TreeSearchNode<P, A>> evaluation, TreeSearchNode<P, A> node, P state) {
            do {
                // Evaluate state with respect to the node.
                var value = evaluation.Evaluate(context, node, state);
                // Visit the node with that evaluation.
                node.Visit(value);

            // Keep moving up the tree while there is a valid parent.
            } while ((node = node.Parent) != null);
        }
        
    }

    /// <summary>
    /// Backpropagation strategy that evaluates the state with respect to the last node before the playout phase and visits each node up to the root node with that evaluation.
    /// </summary>
    /// <typeparam name="D"><see cref="SearchContext{D}"/></typeparam>
    /// <typeparam name="P"><see cref="SearchContext{P}"/></typeparam>
    /// <typeparam name="A"><see cref="SearchContext{A}"/></typeparam>
    /// <typeparam name="S"><see cref="SearchContext{S}"/></typeparam>
    /// <typeparam name="Sol"><see cref="SearchContext{Sol}"/></typeparam>
    public class EvaluateOnceBackPropagation<D, P, A, S, Sol> : ITreeBackPropagation<D, P, A, S, Sol> where D : class where P : State where A : class, IMove where S : class where Sol : class {

        /// <summary>
        /// Propagate an evaluation value of the argument state starting from the argument node back up to the root node.
        /// </summary>
        /// <param name="context">The context of the search.</param>
        /// <param name="evaluation">The strategy used to evaluate the state.</param>
        /// <param name="node">The node from which the backpropagation starts.</param>
        /// <param name="state">The state that should be evaluated.</param>
        public void BackPropagate(SearchContext<D, P, A, S, Sol> context, IStateEvaluation<D, P, A, S, Sol, TreeSearchNode<P, A>> evaluation, TreeSearchNode<P, A> node, P state) {

            // Evaluate the state with respect to the argument node.
            var value = evaluation.Evaluate(context, node, state);
            
            do {
                // Visit the node with the evaluation value.
                node.Visit(value);

            // Keep moving up the tree while there is a valid parent.
            } while ((node = node.Parent) != null);
        }

    }

    /// <summary>
    /// Backpropagation strategy that evaluates the state with respect to the last node before the playout phase.
    /// Each node is visited with the evaluation value, although nodes that are not the root player's nodes are visited with a negated value.
    /// Note: this requires the state evaluation to be done from the perspective of the root player.
    /// </summary>
    /// <typeparam name="D"><see cref="SearchContext{D}"/></typeparam>
    /// <typeparam name="P"><see cref="SearchContext{P}"/></typeparam>
    /// <typeparam name="A"><see cref="SearchContext{A}"/></typeparam>
    /// <typeparam name="S"><see cref="SearchContext{S}"/></typeparam>
    /// <typeparam name="Sol"><see cref="SearchContext{Sol}"/></typeparam>
    public class EvaluateOnceAndColourBackPropagation<D, P, A, S, Sol> : ITreeBackPropagation<D, P, A, S, Sol> where D : class where P : State where A : class, IMove where S : class where Sol : class {

        /// <summary>
        /// Propagate an evaluation value of the argument state starting from the argument node back up to the root node.
        /// </summary>
        /// <param name="context">The context of the search.</param>
        /// <param name="evaluation">The strategy used to evaluate the state.</param>
        /// <param name="node">The node from which the backpropagation starts.</param>
        /// <param name="state">The state that should be evaluated.</param>
        public void BackPropagate(SearchContext<D, P, A, S, Sol> context, IStateEvaluation<D, P, A, S, Sol, TreeSearchNode<P, A>> evaluation, TreeSearchNode<P, A> node, P state) {

            // Evaluate the state with respect to the argument node.
            var value = evaluation.Evaluate(context, node, state);

            // The root player is the current player in the search's source state.
            var rootPlayer = context.Source.CurrentPlayer();
            
            do {
                // Check whether or not this node is a root player's node.
                var targetPlayer = node.IsRoot() || rootPlayer == node.Payload.Player();
                
                // Visits the node with a coloured evaluation value.
                node.Visit(targetPlayer ? value : -value);

            // Keep moving up the tree while there is a valid parent.
            } while ((node = node.Parent) != null);

        }

    }

}
