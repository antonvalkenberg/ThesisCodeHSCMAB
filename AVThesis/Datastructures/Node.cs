using System;
using System.Collections.Generic;

/// <summary>
/// Written by A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by G.J. Roelofs and T. Aliyev.
/// </summary>
namespace AVThesis.Datastructures {

    /// <summary>
    /// The most basic form of a Node.
    /// </summary>
    /// <typeparam name="A">The Type of thing the Node represents (e.g. a state or action).</typeparam>
    public abstract class Node<A> : IEquatable<Node<A>> where A : class {

        #region Properties

        /// <summary>
        /// The Node's subject matter (e.g. a gamestate or gameaction).
        /// </summary>
        public A Payload { get; private set; }

        /// <summary>
        /// The Node that this is a child of.
        /// </summary>
        public Node<A> Parent { get; set; }

        /// <summary>
        /// Collection of Nodes that can be reached from this Node.
        /// </summary>
        public List<Node<A>> Children { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Default Constructor.
        /// </summary>
        public Node() {
            Payload = null;
            Parent = null;
            Children = new List<Node<A>>();
        }

        /// <summary>
        /// Constructor that sets the argument Node as the Parent of the constructed Node.
        /// </summary>
        /// <param name="parent">The parent Node (i.e. the Node that is above the Node to be constructed).</param>
        public Node(Node<A> parent) {
            Payload = null;
            Parent = parent;
            Children = new List<Node<A>>();
        }

        /// <summary>
        /// Constructor that sets the Parent and Payload of the constructed Node.
        /// </summary>
        /// <param name="payload">The payload to be stored.</param>
        /// <param name="parent">See <see cref="Node{A, N}.Node(N)"/></param>
        public Node(A payload, Node<A> parent) {
            Payload = payload;
            Parent = parent;
            Children = new List<Node<A>>();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Determines if this Node is the Root node (i.e. it has no parent).
        /// </summary>
        /// <returns>Whether or not this Node's Parent equals the default value.</returns>
        public bool IsRoot() {
            return Parent == null;
        }

        /// <summary>
        /// Determines if this Node is a Leaf node (i.e. it has no children).
        /// </summary>
        /// <returns>Whether or not the Count of Children equals zero.</returns>
        public bool IsLeaf() {
            return Children.IsNullOrEmpty();
        }

        /// <summary>
        /// Calculates the depth of this Node in relation to the Root.
        /// </summary>
        /// <returns>A number representing the depth of this Node, where the depth of the Root is 0.</returns>
        public int CalculateDepth() {
            int depth = 0;
            Node<A> node = this;
            while (!node.IsRoot()) {
                depth++;
                node = node.Parent;
            }
            return depth;
        }

        /// <summary>
        /// Adds a child to this Node's Children.
        /// </summary>
        /// <param name="child">The Node to add as a child.</param>
        public void AddChild(Node<A> child) {
            Children.Add(child);
        }

        /// <summary>
        /// Determines if this Node is a descendant of the provided Node (i.e. when traversing the tree upwards, if the argument Node is encountered).
        /// </summary>
        /// <param name="ancestor">Node that is a potential ancestor of this Node.</param>
        /// <returns>Whether or not the argument Node is an ancestor of this Node.</returns>
        public bool IsDescendantOf(Node<A> ancestor) {
            // A Node cannot be it's own ancestor
            if (Equals(ancestor)) return false;

            Node<A> node = this;
            while (!node.IsRoot()) {
                node = node.Parent;

                if (node.Equals(ancestor)) return true;
            }

            return false;
        }

        /// <summary>
        /// Determines if this Node is an ancestor of the provided Node (i.e. when traversing the tree upwards, if this Node is encountered).
        /// </summary>
        /// <param name="descendant">Node that is a potential descendant of this Node.</param>
        /// <returns>Whether or not the argument Node is a descendant of this Node.</returns>
        public bool IsAncestorOf(Node<A> descendant) {
            while (!descendant.IsRoot()) {
                if (descendant.Equals(this)) return true;
                descendant = descendant.Parent;
            }

            return false;
        }

        #endregion

        #region Overridden Methods

        public static bool operator ==(Node<A> left, Node<A> right) {
            return Equals(left, right);
        }

        public static bool operator !=(Node<A> left, Node<A> right) {
            return !Equals(left, right);
        }

        public override bool Equals(object obj) {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Node<A>)obj);
        }

        public bool Equals(Node<A> other) {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return GetHashCode() == other.GetHashCode();
        }

        public override int GetHashCode() {
            unchecked { // overflow is fine, the number just wraps
                var hash = (int)Constants.HASH_OFFSET_BASIS;
                hash = Constants.HASH_FNV_PRIME * (hash ^ (Payload != null ? Payload.GetHashCode() : 0));
                foreach (var child in Children) {
                    hash = Constants.HASH_FNV_PRIME * (hash ^ child.GetHashCode());
                }
                return hash;
            }
        }

        #endregion

    }

}
