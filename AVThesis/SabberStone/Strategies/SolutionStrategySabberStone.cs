using System;
using System.Collections.Generic;
using System.Linq;
using AVThesis.Search;
using AVThesis.Search.Tree;
using AVThesis.Search.Tree.MCTS;

/// <summary>
/// Copyright © 2019 Anton Valkenberg
/// Written by BSc. A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by MSc. G.-J. Roelofs, MSc. T. Aliyev and MSc. D. de Rydt.
/// </summary>
namespace AVThesis.SabberStone.Strategies {

    /// <summary>
    /// Represents a way of creating a solution to a search for SabberStone.
    /// </summary>
    public class SolutionStrategySabberStone : ISolutionStrategy<List<SabberStoneAction>, SabberStoneState, SabberStoneAction, object, SabberStoneAction, TreeSearchNode<SabberStoneState, SabberStoneAction>> {

        #region Properties

        /// <summary>
        /// Whether or not the search technique used Hierarchical Expansion.
        /// </summary>
        private bool HierarchicalExpansion { get; }

        /// <summary>
        /// Strategy for evaluating the value of nodes.
        /// </summary>
        private INodeEvaluation<TreeSearchNode<SabberStoneState, SabberStoneAction>> NodeEvaluation { get; }

        /// <summary>
        /// Collection of tasks and values assigned to them during the solution process.
        /// </summary>
        public List<Tuple<SabberStonePlayerTask, double>> TaskValues { get; private set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs a new instance of this SolutionStrategy.
        /// </summary>
        /// <param name="hierarchicalExpansion">Whether or not the search that this SolutionStrategy is for uses Hierarchical Expansion.</param>
        /// <param name="nodeEvaluation">NodeEvaluation strategy.</param>
        public SolutionStrategySabberStone(bool hierarchicalExpansion, INodeEvaluation<TreeSearchNode<SabberStoneState, SabberStoneAction>> nodeEvaluation) {
            HierarchicalExpansion = hierarchicalExpansion;
            NodeEvaluation = nodeEvaluation;
            TaskValues = new List<Tuple<SabberStonePlayerTask, double>>();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Clears the task-values collection.
        /// </summary>
        public void ClearTaskValues() {
            TaskValues = new List<Tuple<SabberStonePlayerTask, double>>();
        }
        
        /// <inheritdoc />
        public SabberStoneAction Solution(SearchContext<List<SabberStoneAction>, SabberStoneState, SabberStoneAction, object, SabberStoneAction> context, TreeSearchNode<SabberStoneState, SabberStoneAction> node) {

            // Check if we're trying to make a solution for a search with Hierarchical Expansion (HE).
            if (HierarchicalExpansion) {

                var solution = new SabberStoneAction();
                var rootPlayerId = context.Source.CurrentPlayer();
                var mcts = (MCTS<List<SabberStoneAction>, SabberStoneState, SabberStoneAction, object, SabberStoneAction>)context.Search;
                var selection = mcts.SelectionStrategy;

                // The final-node selection strategy has chosen a child of the root as final-node.
                // We now have to piece together the complete action until the turn goes to the opponent.
                while (node.Payload.Player() == rootPlayerId) {
                    var task = node.Payload.Tasks.First();

                    solution.AddTask(task);
                    TaskValues.Add(new Tuple<SabberStonePlayerTask, double>(task, node.CalculateScore(NodeEvaluation)));

                    // Move to the next node in the tree, unless we are currently at a leaf node
                    if (node.IsLeaf()) break;
                    node = selection.SelectNextNode(context, node);
                }

                return solution;
            }

            // If not HE, the task values are combined for the action, so just assign the action's value to each task in the list.
            foreach (var payloadTask in node.Payload.Tasks) {
                TaskValues.Add(new Tuple<SabberStonePlayerTask, double>(payloadTask, node.CalculateScore(NodeEvaluation)));
            }

            return node.Payload;
        }

        #endregion

    }
}
