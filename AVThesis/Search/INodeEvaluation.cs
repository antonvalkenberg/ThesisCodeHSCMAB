using AVThesis.Search.Tree;

/// <summary>
/// Copyright © 2019 Anton Valkenberg
/// Written by BSc. A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by MSc. G.-J. Roelofs, MSc. T. Aliyev and MSc. D. de Rydt.
/// </summary>
namespace AVThesis.Search {

    /// <summary>
    /// A way to evaluate a Node.
    /// </summary>
    /// <typeparam name="N">The Type of Node that is to be evaluated.</typeparam>
    public interface INodeEvaluation<in N> {

        /// <summary>
        /// Returns the score associated with the argument node.
        /// </summary>
        /// <param name="node">The node to score.</param>
        /// <returns>Double representing the score associated with the argument node.</returns>
        double Score(N node);

    }

    /// <summary>
    /// Provides a way to evaluate a <see cref="TreeSearchNode{S, A}"/> by using its UCB (Upper Confidence Bounds) value.
    /// </summary>
    /// <typeparam name="P"><see cref="TreeSearchNode{S}"/></typeparam>
    /// <typeparam name="A"><see cref="TreeSearchNode{A}"/></typeparam>
    public class ScoreUCB<P, A> : INodeEvaluation<TreeSearchNode<P, A>> where P : class where A : class {

        #region Properties

        /// <summary>
        /// A constant value to be used in the UCB formula. Note: this value should be tuned experimentally per domain.
        /// </summary>
        public double C { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs a new instance of the ScoreUCB node-evaluation with a specific C-constant.
        /// </summary>
        /// <param name="c">The C-constant to use.</param>
        public ScoreUCB(double c) {
            C = c;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Determines the UCB-score of the argument node.
        /// </summary>
        /// <param name="node">The node to score.</param>
        /// <returns>Double representing the node's UCB score.</returns>
        public double Score(TreeSearchNode<P, A> node) {
            return Util.UCB(node.Score, node.Visits, node.Parent.Visits, C);
        }

        #endregion

    }

    /// <summary>
    /// Evaluates a <see cref="TreeSearchNode{S,A}"/> based on its average score.
    /// </summary>
    /// <typeparam name="P"><see cref="TreeSearchNode{S}"/></typeparam>
    /// <typeparam name="A"><see cref="TreeSearchNode{A}"/></typeparam>
    public class AverageScore<P, A> : INodeEvaluation<TreeSearchNode<P, A>> where P : class where A : class {

        #region Public Methods

        /// <inheritdoc />
        public double Score(TreeSearchNode<P, A> node) {
            if (node.Visits <= 0) return 0;
            return node.Score / node.Visits;
        }

        #endregion

    }

}
