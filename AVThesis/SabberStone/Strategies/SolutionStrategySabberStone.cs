using System.Linq;
using AVThesis.Search;
using AVThesis.Search.Tree;
using AVThesis.Search.Tree.MCTS;
using SabberStoneCore.Tasks;

/// <summary>
/// Written by A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by G.J. Roelofs and T. Aliyev.
/// </summary>
namespace AVThesis.SabberStone.Strategies {

    /// <summary>
    /// Represents a way of creating a solution to a search for SabberStone.
    /// </summary>
    public class SolutionStrategySabberStone : ISolutionStrategy<object, SabberStoneState, SabberStoneAction, object, SabberStoneAction, TreeSearchNode<SabberStoneState, SabberStoneAction>> {

        private bool HierarchicalExpansion { get; }

        public SolutionStrategySabberStone(bool hierarchicalExpansion) {
            HierarchicalExpansion = hierarchicalExpansion;
        }

        /// <inheritdoc cref="ISolutionStrategy{D,P,A,S,Sol,N}.Solution"/>
        public SabberStoneAction Solution(SearchContext<object, SabberStoneState, SabberStoneAction, object, SabberStoneAction> context, TreeSearchNode<SabberStoneState, SabberStoneAction> node) {
            
            // Check if we're trying to make a solution for a search with Hierarchical Expansion.
            if (HierarchicalExpansion) {

                var solution = new SabberStoneAction();
                var rootPlayerId = context.Source.CurrentPlayer();
                var mcts = (MCTS<object, SabberStoneState, SabberStoneAction, object, SabberStoneAction>) context.Search;
                var selection = mcts.SelectionStrategy;

                // The final-node selection strategy has chosen a child of the root as final-node.
                // We now have to piece together the complete action until the turn goes to the opponent.
                while (!node.IsLeaf() && node.Payload.Player() == rootPlayerId) {
                    var task = node.Payload.Tasks.First();
                    solution.AddTask(task);
                    // Move to the next node in the tree
                    node = selection.SelectNextNode(context, node);
                }

                return solution;
            }

            // If not, just return the payload of the node.
            return node.Payload;
        }

    }
}
