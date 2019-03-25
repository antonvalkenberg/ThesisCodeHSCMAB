using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using AVThesis.Datastructures;

/// <summary>
/// Written by A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by G.J. Roelofs and T. Aliyev.
/// </summary>
namespace AVThesis.Search {

    /// <summary>
    /// A Node to be used in search.
    /// </summary>
    /// <typeparam name="S">A Type representing a state in the search.</typeparam>
    /// <typeparam name="A">A Type representing an action in the search.</typeparam>
    public class SearchNode<S, A> : Node<A>, IEquatable<SearchNode<S, A>> where S : class where A : class {

        #region Properties

        /// <summary>
        /// The state that this SearchNode represents.
        /// </summary>
        public S State { get; set; }

        /// <summary>
        /// The score for this SearchNode.
        /// </summary>
        public double Score { get; set; }

        /// <summary>
        /// The PositionGenerator for the actions possible from this SearchNode's state.
        /// </summary>
        public IPositionGenerator<A> PositionGenerator { get; set; }

        /// <summary>
        /// The SearchNode that this is a child of.
        /// </summary>
        public new SearchNode<S, A> Parent { get; set; }

        /// <summary>
        /// Collection of SearchNodes that can be reached from this SearchNode.
        /// </summary>
        public new List<SearchNode<S, A>> Children { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Default Constructor.
        /// </summary>
        public SearchNode() {
            State = null;
            Parent = null;
            Children = new List<SearchNode<S, A>>();
        }

        /// <summary>
        /// Constructor that sets the argument State as this SearchNode's State.
        /// </summary>
        /// <param name="state">The State that this SearchNode represents.</param>
        public SearchNode(S state) {
            State = state;
            Parent = null;
            Children = new List<SearchNode<S, A>>();
        }

        /// <summary>
        /// Constructor that sets the argument Node as the Parent of this SearchNode.
        /// </summary>
        /// <param name="parent">The parent Node (i.e. the Node that is above the SearchNode to be constructed).</param>
        /// <param name="state">See <see cref="SearchNode{S, A, N}.SearchNode(S)"/></param>
        public SearchNode(SearchNode<S, A> parent, S state) {
            State = state;
            Parent = parent;
            Children = new List<SearchNode<S, A>>();
        }

        /// <summary>
        /// Constructor that sets the argument Action as this SearchNode's action.
        /// </summary>
        /// <param name="state">See <see cref="SearchNode{S, A, N}.SearchNode(S)"/></param>
        /// <param name="action">The Action that this SearchNode holds.</param>
        public SearchNode(S state, A action) : base(action, null) {
            State = state;
            Parent = null;
            Children = new List<SearchNode<S, A>>();
        }

        /// <summary>
        /// Constructor that sets an Action as well as a parent Node.
        /// </summary>
        /// <param name="parent">See <see cref="SearchNode{S, A, N}.SearchNode(N, S)"/></param>
        /// <param name="state">See <see cref="SearchNode{S, A, N}.SearchNode(S)"/></param>
        /// <param name="action">See <see cref="SearchNode{S, A, N}.SearchNode(S, A)"/></param>
        public SearchNode(SearchNode<S, A> parent, S state, A action) : base(action, null) {
            State = state;
            Parent = parent;
            Children = new List<SearchNode<S, A>>();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Creates a copy of this SearchNode.
        /// </summary>
        /// <returns>A copy of this SearchNode.</returns>
        public SearchNode<S, A> Copy() {
            var newNode = IsRoot() ? new SearchNode<S, A>(State, Payload) : new SearchNode<S, A>(Parent.Copy(), State, Payload);
            newNode.Children = new List<SearchNode<S, A>>(Children);
            return newNode;
        }

        /// <summary>
        /// Determines if this SearchNode's PositionGenerator exists and cannot advance to the next element.
        /// </summary>
        /// <returns>Whether or not this SearchNode is fully expanded.</returns>
        public bool IsFullyExpanded() {
            return PositionGenerator != null && !PositionGenerator.HasNext();
        }

        /// <summary>
        /// Adds the argument's value to this SearchNode's Score.
        /// </summary>
        /// <param name="value">The value to add.</param>
        public void AddToScore(double value) {
            Score += value;
        }

        /// <summary>
        /// Determines if this SearchNode is the Root node (i.e. it has no parent).
        /// </summary>
        /// <returns>Whether or not this SearchNode's Parent equals the default value.</returns>
        public new bool IsRoot() {
            return Parent == null;
        }

        /// <summary>
        /// Determines if this SearchNode is a Leaf node (i.e. it has no children).
        /// </summary>
        /// <returns>Whether or not the Count of Children equals zero.</returns>
        public new bool IsLeaf() {
            return Children.IsNullOrEmpty();
        }

        /// <summary>
        /// Calculates the depth of this SearchNode in relation to the Root.
        /// </summary>
        /// <returns>A number representing the depth of this SearchNode, where the depth of the Root is 0.</returns>
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
        /// Adds a child to this SearchNode's Children.
        /// </summary>
        /// <param name="child">The SearchNode to add as a child.</param>
        public void AddChild(SearchNode<S, A> child) {
            Children.Add(child);
        }

        /// <summary>
        /// Determines if this SearchNode is a descendant of the provided SearchNode (i.e. when traversing the tree upwards, if the argument SearchNode is encountered).
        /// </summary>
        /// <param name="ancestor">SearchNode that is a potential ancestor of this SearchNode.</param>
        /// <returns>Whether or not the argument SearchNode is an ancestor of this SearchNode.</returns>
        public bool IsDescendantOf(SearchNode<S, A> ancestor) {
            // A SearchNode cannot be it's own ancestor
            if (Equals(ancestor)) return false;

            var node = this;
            while (!node.IsRoot()) {
                node = node.Parent;

                if (node.Equals(ancestor)) return true;
            }

            return false;
        }

        /// <summary>
        /// Determines if this SearchNode is an ancestor of the provided SearchNode (i.e. when traversing the tree upwards, if this SearchNode is encountered).
        /// </summary>
        /// <param name="descendant">SearchNode that is a potential descendant of this SearchNode.</param>
        /// <returns>Whether or not the argument SearchNode is a descendant of this SearchNode.</returns>
        public bool IsAncestorOf(SearchNode<S, A> descendant) {
            while (!descendant.IsRoot()) {
                if (descendant.Equals(this)) return true;
                descendant = descendant.Parent;
            }

            return false;
        }

        #endregion

        #region Overridden Methods

        public static bool operator ==(SearchNode<S, A> left, SearchNode<S, A> right) {
            return Equals(left, right);
        }

        public static bool operator !=(SearchNode<S, A> left, SearchNode<S, A> right) {
            return !Equals(left, right);
        }

        public override bool Equals(object obj) {
            if (obj == null) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((SearchNode<S, A>)obj);
        }

        public bool Equals(SearchNode<S, A> other) {
            if (other == null) return false;
            if (ReferenceEquals(this, other)) return true;
            return GetHashCode() == other.GetHashCode();
        }

        public override int GetHashCode() {
            unchecked { // overflow is fine, the number just wraps
                var hash = (int) AVThesis.Constants.HASH_OFFSET_BASIS;
                hash = AVThesis.Constants.HASH_FNV_PRIME * (hash ^ (Payload != null ? Payload.GetHashCode() : 0));
                hash = AVThesis.Constants.HASH_FNV_PRIME * (hash ^ (State != null ? State.GetHashCode() : 0));
                hash = AVThesis.Constants.HASH_FNV_PRIME * (hash ^ Score.GetHashCode());
                foreach (var child in Children) {
                    hash = AVThesis.Constants.HASH_FNV_PRIME * (hash ^ child.GetHashCode());
                }
                return hash;
            }
        }

        #endregion

        #region Comparers

        /// <summary>
        /// Compares SearchNode objects based on their score.
        /// </summary>
        public class ScoreComparer : IComparer<SearchNode<S, A>> {

            /// <summary>
            /// Can be used to order SearchNode's by descending score.
            /// </summary>
            /// <param name="x"><see cref="ScoreComparer.Compare(SearchNode{S, A}, SearchNode{S, A})"/></param>
            /// <param name="y"><see cref="ScoreComparer.Compare(SearchNode{S, A}, SearchNode{S, A})"/></param>
            /// <returns>An integer indicating whether the score of y is less than, equal to or greater than x's score.</returns>
            public int CompareDescending(SearchNode<S, A> x, SearchNode<S, A> y) {
                return Compare(y, x);
            }

            /// <summary>
            /// Can be used to order SearchNode's by ascending score.
            /// </summary>
            /// <param name="x"><see cref="ScoreComparer.Compare(SearchNode{S, A}, SearchNode{S, A})"/></param>
            /// <param name="y"><see cref="ScoreComparer.Compare(SearchNode{S, A}, SearchNode{S, A})"/></param>
            /// <returns>An integer indicating whether the score of x is less than, equal to or greater than y's score.</returns>
            public int CompareAscending(SearchNode<S, A> x, SearchNode<S, A> y) {
                return Compare(x, y);
            }

            /// <summary>
            /// Returns a positive integer when <paramref name="x"/>'s score is greater than that of <paramref name="y"/>.
            /// Returns zero when the scores of <paramref name="x"/> and <paramref name="y"/> are equal.
            /// Returns a negative integer when <paramref name="x"/>'s score is less than that of <paramref name="y"/>.
            /// </summary>
            /// <param name="x">A SearchNode.</param>
            /// <param name="y">Another SearchNode.</param>
            /// <returns>An integer indicating whether the score of x is less than, equal to or greater than y's score.</returns>
            [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
            public int Compare(SearchNode<S, A> x, SearchNode<S, A> y) {
                return x.Score.CompareTo(y.Score);
            }

        }

        #endregion

    }
}
