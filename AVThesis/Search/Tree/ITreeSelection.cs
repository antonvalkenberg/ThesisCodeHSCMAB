using System;
using System.Collections.Generic;
using System.Linq;
using AVThesis.Datastructures;

/// <summary>
/// Written by A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by G.J. Roelofs and T. Aliyev.
/// </summary>
namespace AVThesis.Search.Tree {

    /// <summary>
    /// Defines what a tree selection strategy should have.
    /// </summary>
    /// <typeparam name="D"><see cref="SearchContext{D}"/></typeparam>
    /// <typeparam name="P"><see cref="SearchContext{P}"/></typeparam>
    /// <typeparam name="A"><see cref="SearchContext{A}"/></typeparam>
    /// <typeparam name="S"><see cref="SearchContext{S}"/></typeparam>
    /// <typeparam name="Sol"><see cref="SearchContext{Sol}"/></typeparam>
    public interface ITreeSelection<D, P, A, S, Sol> where D : class where P : State where A : class where S : class where Sol : class {

        /// <summary>
        /// Selects the next node, given the argument node.
        /// </summary>
        /// <param name="context">The context of the search.</param>
        /// <param name="node">The node from which to select the next node.</param>
        /// <returns>The next node.</returns>
        TreeSearchNode<P, A> SelectNextNode(SearchContext<D, P, A, S, Sol> context, TreeSearchNode<P, A> node);

    }

    /// <summary>
    /// Selection strategy that selects the best node based on a provided node evaluation strategy, with a minimum number of visits before any evaluation is done.
    /// </summary>
    /// <typeparam name="D"><see cref="SearchContext{D}"/></typeparam>
    /// <typeparam name="P"><see cref="SearchContext{P}"/></typeparam>
    /// <typeparam name="A"><see cref="SearchContext{A}"/></typeparam>
    /// <typeparam name="S"><see cref="SearchContext{S}"/></typeparam>
    /// <typeparam name="Sol"><see cref="SearchContext{Sol}"/></typeparam>
    public class BestNodeSelection<D, P, A, S, Sol> : ITreeSelection<D, P, A, S, Sol> where D : class where P : State where A : class where S : class where Sol : class {

        /// <summary>
        /// Compares nodes according to a provided node evaluation strategy.
        /// </summary>
        public class NodeComparer : IComparer<TreeSearchNode<P, A>> {

            #region Fields

            private INodeEvaluation<TreeSearchNode<P, A>> _nodeEvaluation;

            #endregion

            #region Constructor

            /// <summary>
            /// Constructs a new instance.
            /// </summary>
            /// <param name="nodeEvaluation">The strategy of nove evaluation to use.</param>
            public NodeComparer(INodeEvaluation<TreeSearchNode<P, A>> nodeEvaluation) {
                _nodeEvaluation = nodeEvaluation;
            }

            #endregion

            #region Public Methods

            /// <summary>
            /// Compares two nodes and returns a value indicating whether one is less than, equal to, or greater than the other.
            /// </summary>
            /// <param name="x">The first node to compare.</param>
            /// <param name="y">The second node to compare.</param>
            /// <returns>A signed integer that indicates the relative values of x and y, as shown in the following table.
            /// Value Meaning
            /// Less than zero -> x is less than y.
            /// Zero -> x equals y.
            /// Greater than zero -> x is greater than y.</returns>
            public int Compare(TreeSearchNode<P, A> x, TreeSearchNode<P, A> y) {
                return y.CalculateScore(_nodeEvaluation).CompareTo(x.CalculateScore(_nodeEvaluation));
            }

            #endregion

        }

        #region Fields

        private int _minVisits;
        private INodeEvaluation<TreeSearchNode<P, A>> _nodeEvaluation;
        private NodeComparer _nodeComparer;

        #endregion

        #region Properties

        /// <summary>
        /// The minimum number of visits before using the node evaluation to select the best node.
        /// </summary>
        public int MinVisits { get => _minVisits; set => _minVisits = value; }
        /// <summary>
        /// The strategy for evaluation the value of nodes.
        /// </summary>
        public INodeEvaluation<TreeSearchNode<P, A>> NodeEvaluation { get => _nodeEvaluation; set => _nodeEvaluation = value; }
        /// <summary>
        /// A way to compare between nodes.
        /// </summary>
        public NodeComparer NodeComparer1 { get => _nodeComparer; set => _nodeComparer = value; }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        /// <param name="minVisits">The minimum number of visits before using the node evaluation to select the best node.</param>
        /// <param name="nodeEvaluation">The strategy for evaluation the value of nodes.</param>
        public BestNodeSelection(int minVisits, INodeEvaluation<TreeSearchNode<P, A>> nodeEvaluation) {
            MinVisits = minVisits;
            NodeEvaluation = nodeEvaluation;
            NodeComparer1 = new NodeComparer(nodeEvaluation);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Selects the next node, given the argument node.
        /// </summary>
        /// <param name="context">The context of the search.</param>
        /// <param name="node">The node from which to select the next node.</param>
        /// <returns>The next node.</returns>
        public TreeSearchNode<P, A> SelectNextNode(SearchContext<D, P, A, S, Sol> context, TreeSearchNode<P, A> node) {
            // Determine the minimum number of visits on the parent node required before using evaluation.
            int numberOfChildren = node.Children.Count;
            int minVisitsOnParent = MinVisits * numberOfChildren;

            // In default behaviour, we will have iterated over all children once before arriving at the first call to the Selection Strategy.
            // If the parent node hasn't been visited a minimum number of times, select the next appropriate child.
            if (minVisitsOnParent > node.Visits) {
                return node.Children.ElementAt(node.Visits % numberOfChildren);
            } else if (minVisitsOnParent == node.Visits) {
                node.Children = node.Children.OrderByDescending(i => i.CalculateScore(NodeEvaluation)).ToList();
            } else {
                // The first child is always the one picked; so it is the only node we need to sort to a new location.
                // Pick it, and ensure it's at its required location.
                var firstChild = node.Children.First();

                // Update the score and remove dirty.
                firstChild.CalculateScore(NodeEvaluation);

                int i = 1;
                for (; i < node.Children.Count; i++) {
                    if (firstChild.CalculateScore(NodeEvaluation) >= node.Children.ElementAt(i).CalculateScore(NodeEvaluation))
                        break;
                }
                i--;

                // Move everyone by one, and set the child at its newest index.
                if (i>0) {
                    if (i == 1) {
                        // Special case where we optimise for when we are just better than the second item (often).
                        var items = node.Children.ToArray();
                        items[0] = items[1];
                        items[1] = firstChild;
                        node.Children = items.ToList();
                    } else {
                        var items = node.Children.ToArray();
                        Array.Copy(items, 1, items, 0, i);
                        items[i] = firstChild;
                        node.Children = items.ToList();
                    }
                }
            }

            return node.Children.First();
        }

        #endregion

    }

}
