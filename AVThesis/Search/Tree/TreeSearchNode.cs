using System;
using System.Collections.Generic;
using AVThesis.Datastructures;

/// <summary>
/// Written by A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by G.J. Roelofs and T. Aliyev.
/// </summary>
namespace AVThesis.Search.Tree {

    /// <summary>
    /// A SearchNode that is used in search in trees.
    /// </summary>
    /// <typeparam name="S">A Type representing a state in the search.</typeparam>
    /// <typeparam name="A">A Type representing an action in the search.</typeparam>
    public class TreeSearchNode<S, A> : SearchNode<S, A> where S : class where A : class {

        #region Fields

        private int _visits = 0;
        private TreeSearchNode<S, A> _parent;
        private List<TreeSearchNode<S, A>> _children;

        /// These fields are mainly for caching optimizations; used for determining when to update the score of a node and/or its position in the position-enumerator.
        private bool _dirty = true;
        private double _evaluatedScore = 0;
        private double _minimumChildScore = Double.MaxValue;
        private double _maximumChildScore = Double.MinValue;

        #endregion

        #region Properties

        /// <summary>
        /// The amount of visits this node has had.
        /// </summary>
        public int Visits { get => _visits; set => _visits = value; }

        /// <summary>
        /// The TreeSearchNode that this is a child of.
        /// </summary>
        public new TreeSearchNode<S, A> Parent { get => _parent; set => _parent = value; }

        /// <summary>
        /// Collection of TreeSearchNodes that can be reached from this TreeSearchNode.
        /// </summary>
        public new List<TreeSearchNode<S, A>> Children { get => _children; set => _children = value; }


        /// <summary>
        /// Whether or not the score of this node is reliable.
        /// </summary>
        public bool Dirty { get => _dirty; private set => _dirty = value; }
        /// <summary>
        /// The score of this node as evaluated by a node evaluation strategy.
        /// </summary>
        private double EvaluatedScore { get => _evaluatedScore; set => _evaluatedScore = value; }
        /// <summary>
        /// The minimum score of any of this node's children.
        /// </summary>
        public double MinimumChildScore { get => _minimumChildScore; set => _minimumChildScore = value; }
        /// <summary>
        /// The maximum score of any of this node's children.
        /// </summary>
        public double MaximumChildScore { get => _maximumChildScore; set => _maximumChildScore = value; }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor that creates a TreeSearchNode without an internal state. The state should be constructor from the root/parent state and the provided action.
        /// </summary>
        /// <param name="payload">The action that this TreeSearchNode holds.</param>
        public TreeSearchNode(A payload) : base(null, payload) {
            Parent = null;
            Children = new List<TreeSearchNode<S, A>>();
        }

        /// <summary>
        /// Constructor that creates a TreeSearchNode with a world-state. This should be viewed as the root node constructor, as subsequent node's states can be constructed using their payload.
        /// </summary>
        /// <param name="state">The state that this TreeSearchNode represents.</param>
        /// <param name="payload"><see cref="TreeSearchNode{S, A}.TreeSearchNode(A)"/></param>
        public TreeSearchNode(S state, A payload) : base(state, payload) {
            Parent = null;
            Children = new List<TreeSearchNode<S, A>>();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns the evaluated score of this node, or if the node is dirty calculates a new score first and also updates max/min child scores for parent.
        /// </summary>
        /// <param name="evaluator">A node evaluation implementation.</param>
        /// <returns>Double value representing the score of this node.</returns>
        public double CalculateScore(INodeEvaluation<TreeSearchNode<S, A>> evaluator) {

            if (Dirty) {
                EvaluatedScore = evaluator.Score(this);
                Dirty = false;

                if (!IsRoot()) {
                    Parent.MinimumChildScore = Math.Min(Parent.MinimumChildScore, EvaluatedScore);
                    Parent.MaximumChildScore = Math.Max(Parent.MaximumChildScore, EvaluatedScore);
                }
            }

            return EvaluatedScore;

        }

        /// <summary>
        /// Visit this node; increase its score with the provided score and increment visit count.
        /// </summary>
        /// <param name="score">The score to add to this node's score.</param>
        public void Visit(double score) {
            Visits++;
            Score += score;
            Dirty = true;
        }

        /// <summary>
        /// Determines if this TreeSearchNode is the Root node (i.e. it has no parent).
        /// </summary>
        /// <returns>Whether or not this TreeSearchNode's Parent equals the default value.</returns>
        public new bool IsRoot() {
            return Parent == null;
        }

        /// <summary>
        /// Determines if this TreeSearchNode is a Leaf node (i.e. it has no children).
        /// </summary>
        /// <returns>Whether or not the Count of Children equals zero.</returns>
        public new bool IsLeaf() {
            return Children.IsNullOrEmpty();
        }

        /// <summary>
        /// Calculates the depth of this TreeSearchNode in relation to the Root.
        /// </summary>
        /// <returns>A number representing the depth of this TreeSearchNode, where the depth of the Root is 0.</returns>
        public new int CalculateDepth() {
            int depth = 0;
            TreeSearchNode<S, A> node = this;
            while (!node.IsRoot()) {
                depth++;
                node = node.Parent;
            }
            return depth;
        }

        /// <summary>
        /// Adds a child to this TreeSearchNode's Children.
        /// </summary>
        /// <param name="child">The TreeSearchNode to add as a child.</param>
        public void AddChild(TreeSearchNode<S, A> child) {
            _children.Add(child);
        }

        /// <summary>
        /// Determines if this TreeSearchNode is a descendant of the provided TreeSearchNode (i.e. when traversing the tree upwards, if the argument TreeSearchNode is encountered).
        /// </summary>
        /// <param name="ancestor">TreeSearchNode that is a potential ancestor of this TreeSearchNode.</param>
        /// <returns>Whether or not the argument TreeSearchNode is an ancestor of this TreeSearchNode.</returns>
        public bool IsDescendantOf(TreeSearchNode<S, A> ancestor) {
            // A TreeSearchNode cannot be it's own ancestor
            if (Equals(ancestor)) return false;

            TreeSearchNode<S, A> node = this;
            while (!node.IsRoot()) {
                node = node.Parent;

                if (node.Equals(ancestor)) return true;
            }

            return false;
        }

        /// <summary>
        /// Determines if this TreeSearchNode is an ancestor of the provided TreeSearchNode (i.e. when traversing the tree upwards, if this TreeSearchNode is encountered).
        /// </summary>
        /// <param name="descendant">TreeSearchNode that is a potential descendant of this TreeSearchNode.</param>
        /// <returns>Whether or not the argument TreeSearchNode is a descendant of this TreeSearchNode.</returns>
        public bool IsAncestorOf(TreeSearchNode<S, A> descendant) {
            while (!descendant.IsRoot()) {
                if (descendant.Equals(this)) return true;
                descendant = descendant.Parent;
            }

            return false;
        }

        #endregion

        #region Overridden Methods

        /// <summary>
        /// Determines if this TreeSearchNode is equal to another by using the equality operator on their <see cref="TreeSearchNode{S, A}.GetHashCode"/>.
        /// </summary>
        /// <param name="other">The TreeSearchNode to equate this one to.</param>
        /// <returns>Whether or not the hashcodes of these two objects are equal.</returns>
        public bool Equals(TreeSearchNode<S, A> other) {
            return GetHashCode() == other.GetHashCode();
        }

        public override int GetHashCode() {
            //TODO calculate correct HashCode for TreeSearchNode
            return base.GetHashCode();
        }

        public override string ToString() {
            return string.Format("[TSN, Children: {0}, Depth: {1}, Payload: {2}, Visits: {3}, Score: {4}, EvaluatedScore: {5}]", Children.Count, CalculateDepth(), Payload, Visits, Score, EvaluatedScore);
        }

        #endregion
    }

}
