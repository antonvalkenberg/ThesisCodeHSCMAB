using System.Collections.Generic;
using AVThesis.Datastructures;

/// <summary>
/// Written by A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by G.J. Roelofs and T. Aliyev.
/// </summary>
namespace AVThesis.Search.Tree {

    /// <summary>
    /// Defines what a strategy should have to expand a node in a search tree.
    /// </summary>
    /// <typeparam name="D"><see cref="SearchContext{D}"/></typeparam>
    /// <typeparam name="P"><see cref="SearchContext{P}"/></typeparam>
    /// <typeparam name="A"><see cref="SearchContext{A}"/></typeparam>
    /// <typeparam name="S"><see cref="SearchContext{S}"/></typeparam>
    /// <typeparam name="Sol"><see cref="SearchContext{Sol}"/></typeparam>
    public interface ITreeExpansion<D, P, A, S, Sol> where D : class where P : State where A : class where S : class where Sol : class {

        /// <summary>
        /// Expands a node and returns the first node selected. Note: this could be the argument node if no selection was deemed required.
        /// </summary>
        /// <param name="context">The context of the search.</param>
        /// <param name="node">The node that is to be expanded.</param>
        /// <param name="state">The state to expand from.</param>
        /// <returns>The first node selected after expansion.</returns>
        TreeSearchNode<P, A> Expand(SearchContext<D, P, A, S, Sol> context, TreeSearchNode<P, A> node, P state);

    }

    /// <summary>
    /// Tree Expansion strategy that only expands a node after it has been visited T number of times.
    /// </summary>
    /// <typeparam name="D"><see cref="SearchContext{D}"/></typeparam>
    /// <typeparam name="P"><see cref="SearchContext{P}"/></typeparam>
    /// <typeparam name="A"><see cref="SearchContext{A}"/></typeparam>
    /// <typeparam name="S"><see cref="SearchContext{S}"/></typeparam>
    /// <typeparam name="Sol"><see cref="SearchContext{Sol}"/></typeparam>
    public class MinimumTExpansion<D, P, A, S, Sol> : ITreeExpansion<D, P, A, S, Sol> where D : class where P : State where A : class where S : class where Sol : class {

        #region Fields

        private int _t;

        #endregion

        #region Properties

        /// <summary>
        /// The minimum amount of times a node has to be visited before it can be expanded.
        /// </summary>
        public int T { get => _t; set => _t = value; }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        /// <param name="t">The minimum amount of times a node has to be visited before it can be expanded.</param>
        public MinimumTExpansion(int t) {
            T = t;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// If the argument node has been visited at least T times, the node is expanded by advancing the PositionGenerator (or creating one if it is undefined) and adding the new child to the node.
        /// </summary>
        /// <param name="context">The context of the search.</param>
        /// <param name="node">The node that is to be expanded.</param>
        /// <param name="state">The state to expand from.</param>
        /// <returns>The argument node if it has been visited less than T time, or if no more expansion is possible, otherwise the newly created child node.</returns>
        public TreeSearchNode<P, A> Expand(SearchContext<D, P, A, S, Sol> context, TreeSearchNode<P, A> node, P state) {

            // No expansion before T visits, except for the root.
            if (node.Visits < T && !node.IsRoot()) return node;

            // Create a position generator if there is not one set in the node yet.
            IPositionGenerator<A> positionGenerator = node.PositionGenerator;
            if (positionGenerator == null) {
                var expansion = context.Expansion;
                positionGenerator = expansion.Expand(context, state);
                node.PositionGenerator = positionGenerator;
            }

            // Move the PositionGenerator to the next item, if available (note: PositionGenerator initialises to before the first item).
            if (positionGenerator.MoveNext()) {
                var child = new TreeSearchNode<P, A>(positionGenerator.Current);
                node.AddChild(child);
                child.Parent = node;
                return child;
            } else {
                return node;
            }

        }

        #endregion

    }

}
