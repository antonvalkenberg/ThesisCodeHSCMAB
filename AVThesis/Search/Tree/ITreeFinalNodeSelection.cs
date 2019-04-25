using System;
using System.Linq;

/// <summary>
/// Copyright © 2019 Anton Valkenberg
/// Written by BSc. A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by MSc. G.-J. Roelofs, MSc. T. Aliyev and MSc. D. de Rydt.
/// </summary>
namespace AVThesis.Search.Tree {

    /// <summary>
    /// Defines what a final node selection strategy should have.
    /// </summary>
    /// <typeparam name="D"><see cref="SearchContext{D}"/></typeparam>
    /// <typeparam name="P"><see cref="SearchContext{P}"/></typeparam>
    /// <typeparam name="A"><see cref="SearchContext{A}"/></typeparam>
    /// <typeparam name="S"><see cref="SearchContext{S}"/></typeparam>
    /// <typeparam name="Sol"><see cref="SearchContext{Sol}"/></typeparam>
    public interface ITreeFinalNodeSelection<D, P, A, S, Sol> where D : class where P : State where A : class where S : class where Sol : class {

        /// <summary>
        /// Returns the 'best' child node of the argument node.
        /// </summary>
        /// <param name="context">The context of the search.</param>
        /// <param name="node">The node from which to select the final node.</param>
        /// <returns>The 'best' child node of the argument node.</returns>
        TreeSearchNode<P, A> SelectFinalNode(SearchContext<D, P, A, S, Sol> context, TreeSearchNode<P, A> node);

    }

    /// <summary>
    /// Final node selection strategy that selects the child node with the best ratio of score to visits.
    /// </summary>
    /// <typeparam name="D"><see cref="SearchContext{D}"/></typeparam>
    /// <typeparam name="P"><see cref="SearchContext{P}"/></typeparam>
    /// <typeparam name="A"><see cref="SearchContext{A}"/></typeparam>
    /// <typeparam name="S"><see cref="SearchContext{S}"/></typeparam>
    /// <typeparam name="Sol"><see cref="SearchContext{Sol}"/></typeparam>
    public class BestRatioFinalNodeSelection<D, P, A, S, Sol> : ITreeFinalNodeSelection<D, P, A, S, Sol> where D : class where P : State where A : class where S : class where Sol : class {

        /// <summary>
        /// Returns the child node of the argument node that has the best score to visits ratio.
        /// </summary>
        /// <param name="context">The context of the search.</param>
        /// <param name="node">The node from which to select the best child.</param>
        /// <returns>The child node of the argument node that has the best score to visits ratio.</returns>
        public TreeSearchNode<P, A> SelectFinalNode(SearchContext<D, P, A, S, Sol> context, TreeSearchNode<P, A> node) {

            var max = double.MinValue;
            var numberOfChildren = node.Children.Count;
            // This makes sure a random node is returned if all ratios are equal.
            var maxIndex = new Random().Next(numberOfChildren);

            for (var i = 0; i < numberOfChildren; i++) {
                var child = node.Children.ElementAt(i);

                // Don't consider nodes without visits (also to avoid divide-by-zero).
                if (child.Visits == 0) continue;

                var nodeScore = child.Score;
                var childRatio = nodeScore / child.Visits;

                if (!(childRatio > max)) continue;

                max = childRatio;
                maxIndex = i;
            }

            // Return the child with the maximum ratio.
            return node.Children.ElementAt(maxIndex);
        }

    }

}
