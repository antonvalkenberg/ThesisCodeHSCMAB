using AVThesis.Search.Tree;

/// <summary>
/// Written by A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by G.J. Roelofs and T. Aliyev.
/// </summary>
namespace AVThesis.Search {

    /// <summary>
    /// A way to evaluate a Node.
    /// </summary>
    /// <typeparam name="N">The Type of Node that is to be evaluated.</typeparam>
		public interface INodeEvaluation<N> {

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

        #region Fields

        private double _C;

        #endregion

        #region Properties

        /// <summary>
        /// A constant value to be used in the UCB formula. Note: this value should be tuned experimentally per domain.
        /// </summary>
        public double C { get => _C; set => _C = value; }

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

}
