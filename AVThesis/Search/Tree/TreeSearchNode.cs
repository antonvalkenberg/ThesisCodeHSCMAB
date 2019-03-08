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
    public class TreeSearchNode<S, A> : SearchNode<S, A>, IEquatable<TreeSearchNode<S, A>> where S : class where A : class {

        #region Properties

        /// <summary>
        /// The amount of visits this node has had.
        /// </summary>
        public int Visits { get; set; }

        /// <summary>
        /// The TreeSearchNode that this is a child of.
        /// </summary>
        public new TreeSearchNode<S, A> Parent { get; set; }

        /// <summary>
        /// Collection of TreeSearchNodes that can be reached from this TreeSearchNode.
        /// </summary>
        public new List<TreeSearchNode<S, A>> Children { get; set; }

        /// <summary>
        /// Whether or not the score of this node is reliable.
        /// </summary>
        public bool Dirty { get; private set; } = true;

        /// <summary>
        /// The score of this node as evaluated by a node evaluation strategy.
        /// </summary>
        private double EvaluatedScore { get; set; }

        /// <summary>
        /// The minimum score of any of this node's children.
        /// </summary>
        public double MinimumChildScore { get; set; } = double.MaxValue;

        /// <summary>
        /// The maximum score of any of this node's children.
        /// </summary>
        public double MaximumChildScore { get; set; } = double.MinValue;

        #endregion

        #region Constructors

        /// <summary>
        /// Default constructor.
        /// </summary>
        public TreeSearchNode() { }

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
        public TreeSearchNode(S state) : base(state) {
            Parent = null;
            Children = new List<TreeSearchNode<S, A>>();
        }

        /// <summary>
        /// Constructor that creates a TreeSearchNode with a world-state and a payload.
        /// </summary>
        /// <param name="state">The state that this TreeSearchNode represents.</param>
        /// <param name="payload"><see cref="Node{A}.Payload"/></param>
        public TreeSearchNode(S state, A payload) : base(state, payload) {
            Parent = null;
            Children = new List<TreeSearchNode<S, A>>();
        }

        /// <summary>
        /// Constructor that creates a TreeSearchNode with a world-state and a parent node.
        /// </summary>
        /// <param name="parent">The parent node of the node.</param>
        /// <param name="state">The state that this TreeSearchNode represents.</param>
        /// <param name="payload"><see cref="Node{A}.Payload"/></param>
        public TreeSearchNode(SearchNode<S, A> parent, S state, A payload) : base(parent, state, payload) {
            Children = new List<TreeSearchNode<S, A>>();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns the evaluated score of this node, or if the node is dirty calculates a new score first and also updates max/min child scores for parent.
        /// </summary>
        /// <param name="evaluation">A node evaluation implementation.</param>
        /// <returns>Double value representing the score of this node.</returns>
        public double CalculateScore(INodeEvaluation<TreeSearchNode<S, A>> evaluation) {
            if (!Dirty) return EvaluatedScore;

            EvaluatedScore = evaluation.Score(this);
            Dirty = false;

            if (!IsRoot()) {
                Parent.MinimumChildScore = Math.Min(Parent.MinimumChildScore, EvaluatedScore);
                Parent.MaximumChildScore = Math.Max(Parent.MaximumChildScore, EvaluatedScore);
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
            var depth = 0;
            var node = this;
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
            Children.Add(child);
        }

        /// <summary>
        /// Determines if this TreeSearchNode is a descendant of the provided TreeSearchNode (i.e. when traversing the tree upwards, if the argument TreeSearchNode is encountered).
        /// </summary>
        /// <param name="ancestor">TreeSearchNode that is a potential ancestor of this TreeSearchNode.</param>
        /// <returns>Whether or not the argument TreeSearchNode is an ancestor of this TreeSearchNode.</returns>
        public bool IsDescendantOf(TreeSearchNode<S, A> ancestor) {
            // A TreeSearchNode cannot be it's own ancestor
            if (Equals(ancestor)) return false;

            var node = this;
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

        public static bool operator ==(TreeSearchNode<S, A> left, TreeSearchNode<S, A> right) {
            return Equals(left, right);
        }

        public static bool operator !=(TreeSearchNode<S, A> left, TreeSearchNode<S, A> right) {
            return !Equals(left, right);
        }

        public override bool Equals(object obj) {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((TreeSearchNode<S, A>)obj);
        }

        public bool Equals(TreeSearchNode<S, A> other) {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return GetHashCode() == other.GetHashCode();
        }

        public override int GetHashCode() {
            unchecked { // overflow is fine, the number just wraps
                var hash = (int)AVThesis.Constants.HASH_OFFSET_BASIS;
                hash = AVThesis.Constants.HASH_FNV_PRIME * (hash ^ (Payload != null ? Payload.GetHashCode() : 0));
                hash = AVThesis.Constants.HASH_FNV_PRIME * (hash ^ (State != null ? State.GetHashCode() : 0));
                hash = AVThesis.Constants.HASH_FNV_PRIME * (hash ^ Score.GetHashCode());
                hash = AVThesis.Constants.HASH_FNV_PRIME * (hash ^ Visits);
                foreach (var child in Children) {
                    hash = AVThesis.Constants.HASH_FNV_PRIME * (hash ^ child.GetHashCode());
                }
                return hash;
            }
        }

        public override string ToString() {
            return $"[TSN, Children: {Children.Count}, Depth: {CalculateDepth()}, Payload: {Payload}, Visits: {Visits}, Score: {Score}, EvaluatedScore: {EvaluatedScore}]";
        }

        #endregion

    }

}
