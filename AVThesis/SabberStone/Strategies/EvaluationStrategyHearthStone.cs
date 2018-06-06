using System;
using System.Linq;
using AVThesis.Search;
using AVThesis.Search.Tree;

/// <summary>
/// Written by A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by G.J. Roelofs and T. Aliyev.
/// </summary>
namespace AVThesis.SabberStone.Strategies {

    /// <summary>
    /// Represents a way of evaluating a board state in HearthStone.
    /// </summary>
    public class EvaluationStrategyHearthStone : IStateEvaluation<object, SabberStoneState, SabberStoneAction, object, SabberStoneAction, TreeSearchNode<SabberStoneState, SabberStoneAction>> {

        #region Public Methods

        /// <summary>
        /// Returns the value of the argument state with respect to the argument node.
        /// </summary>
        /// <param name="context">The context of the search.</param>
        /// <param name="node">The node that provides the context to evaluate the state.</param>
        /// <param name="state">The state that should be evaluated.</param>
        /// <returns>Double representing the value of the state with respect to the node.</returns>
        public double Evaluate(SearchContext<object, SabberStoneState, SabberStoneAction, object, SabberStoneAction> context, TreeSearchNode<SabberStoneState, SabberStoneAction> node, SabberStoneState state) {

            var rootPlayerId = context.Source.CurrentPlayer();
            var rootPlayer = state.Player1.Id == rootPlayerId ? state.Player1 : state.Player2;
            var opponent = rootPlayer.Opponent;

            // Check for a win/loss
            if (state.PlayerWon != State.DRAW) {
                return state.PlayerWon == rootPlayerId ? 1 : -1;
            }

            // Some thoughts:
            //      Minion toughness + attack values
            //          Complexity -> higher health is worth more when power is also higher
            //      Some properties:
            //          # cards in play
            //          # cards in hand
            //          # health points remaining
            //          # cards in deck
            //      Weapons should also be factored in
            //      Determine TTL (turns til lethal) with those properties
            //      TTL - opponent's TTL
            //          Is that the metric we want to check?

            // Note: these stats should be calculated from the root player's perspective.

            // Get the player's minions and some stats about them
            var playerMinions = rootPlayer.BoardZone.GetAll();
            var playerMinionsAttack = playerMinions.Where(i => i.CanAttack).Sum(j => j.AttackDamage);
            var playerMinionsTauntHealth = playerMinions.Where(i => i.HasTaunt).Sum(j => j.Health);
            var opponentMinions = opponent.BoardZone.GetAll();
            var opponentMinionsAttack = opponentMinions.Where(i => i.CanAttack).Sum(j => j.AttackDamage);
            var opponentMinionsTauntHealth = opponentMinions.Where(i => i.HasTaunt).Sum(j => j.Health);

            // Get some stats about player's resources
            var playerCardsInHand = rootPlayer.HandZone.Count;
            var opponentCardsInHand = opponent.HandZone.Count;
            var playerHealth = rootPlayer.Hero.Health;
            var opponentHealth = opponent.Hero.Health;
            var playerCardsInDeck = rootPlayer.DeckZone.Count;
            var opponentCardsInDeck = opponent.DeckZone.Count;
            var playerWeaponDamage = rootPlayer.Hero.Weapon?.AttackDamage * rootPlayer.Hero.Weapon?.Durability ?? 0;
            var opponentWeaponDamage = opponent.Hero.Weapon?.AttackDamage * opponent.Hero.Weapon?.Durability ?? 0;

            // Now use these stats to come to a number....


            //TODO implement Hearthstone state evaluation
            return playerHealth == opponentHealth ? 0 : playerHealth > opponentHealth ? .5 : -.5;
        }

        #endregion

    }
}
