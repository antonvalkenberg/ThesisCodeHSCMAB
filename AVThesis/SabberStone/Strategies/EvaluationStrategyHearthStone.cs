using System;
using System.Collections.Generic;
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
    public class EvaluationStrategyHearthStone : IStateEvaluation<List<SabberStoneAction>, SabberStoneState, SabberStoneAction, object, SabberStoneAction, TreeSearchNode<SabberStoneState, SabberStoneAction>> {

        #region Public Methods

        /// <summary>
        /// Returns the value of the argument state with respect to the argument node.
        /// </summary>
        /// <param name="context">The context of the search.</param>
        /// <param name="node">The node that provides the context to evaluate the state.</param>
        /// <param name="state">The state that should be evaluated.</param>
        /// <returns>Double representing the value of the state with respect to the node.</returns>
        public double Evaluate(SearchContext<List<SabberStoneAction>, SabberStoneState, SabberStoneAction, object, SabberStoneAction> context, TreeSearchNode<SabberStoneState, SabberStoneAction> node, SabberStoneState state) {

            var rootPlayerId = context.Source.CurrentPlayer();
            var rootPlayer = state.Player1.Id == rootPlayerId ? state.Player1 : state.Player2;
            var opponent = rootPlayer.Opponent;

            // Check for a win/loss
            if (state.PlayerWon != State.DRAW) {
                return state.PlayerWon == rootPlayerId ? 1 : -1;
            }

            // Gather stats that we need
            // TODO gather stats from cards in hand
            // TODO think about board control when both player HPs are high

            // Opponent HP
            var oH = opponent.Hero.Health;
            // Opponent's Taunt Minions HP
            var opponentMinions = opponent.BoardZone.GetAll();
            var oMtH = opponentMinions.Where(i => i.HasTaunt).Sum(j => j.Health);
            // Opponent's Unknown HP in Hand
            var oUHh = 0;
            // Opponent's Unknown Direct Damage in Hand
            var oDdh = 0;
            // Opponent's Minion Power
            var oMP = opponentMinions.Where(i => i.CanAttack).Sum(j => j.AttackDamage);
            // Opponent's Unknown Minion Power from Hand
            var oUMPh = 0;
            // Opponent's Weapon Damage
            var oWD = opponent.Hero.Weapon?.AttackDamage ?? 0;
            // Opponent's Fatigue Damage
            var oFD = opponent.DeckZone.IsEmpty ? opponent.Hero.Fatigue + 1 : 0;

            // Root Player HP
            var rH = rootPlayer.Hero.Health;
            // Root Player's Taunt Minions HP
            var rootPlayerMinions = rootPlayer.BoardZone.GetAll();
            var rMtH = rootPlayerMinions.Where(i => i.HasTaunt).Sum(j => j.Health);
            // Root Player's HP in Hand
            var rHh = 0;
            // Root Player's Direct Damage in Hand
            var rDdh = 0;
            // Root Player's Minion Power
            var rMP = rootPlayerMinions.Where(i => i.CanAttack).Sum(j => j.AttackDamage);
            // Root Player's Minion Power from Hand
            var rMPh = 0;
            // Root Player's Weapon Damage
            var rWD = rootPlayer.Hero.Weapon?.AttackDamage ?? 0;
            // Root Player's Fatigue Damage
            var rFD = rootPlayer.DeckZone.IsEmpty ? rootPlayer.Hero.Fatigue + 1 : 0;

            // Calculate the approximate turns before the opponent dies
            var opponentHealth = oH + oMtH + oUHh - oFD - rDdh;
            var rootPlayerDamage = rMP + rMPh + rWD;
            var oTD = rootPlayerDamage > 0 ? opponentHealth / (rootPlayerDamage * 1.0) : int.MaxValue;
            // Calculate the approximate turns before the root player dies
            var rootPlayerHealth = rH + rMtH + rHh - rFD - oDdh;
            var opponentDamage = oMP + oUMPh + oWD;
            var rTD = opponentDamage > 0 ? rootPlayerHealth / (opponentDamage * 1.0) : int.MaxValue;

            // Check some situations
            var canKillOpponentThisTurn = (int) Math.Ceiling(oTD) == 1;
            var canBeKilledByOpponentThisTurn = (int) Math.Ceiling(rTD) == 1;
            var notARaceSituation = oTD >= 4 && rTD >= 4;

            // If the root player can kill the opponent, evaluation is 1
            if (canKillOpponentThisTurn) return 1;
            // If opponent can't be killed, but they can kill root player next turn, evaluation is -1
            if (canBeKilledByOpponentThisTurn) return -1;
            // If this is not a racing situation (yet), return a cautious number
            if (notARaceSituation) return 0;
            
            // If none of the above applies, look at the difference between when the opponent dies and when the root player dies
            var difference = oTD - rTD;
            
            // If the difference is between -1 and 1, it is too close to tell
            if (difference >= -1 && difference <= 1) return 0;
            // If the difference is negative, it means the root player would die later than the opponent, so the root player would be slightly ahead
            if (difference < -1) return 0.5;
            // If the difference is positive, it means the opponent would die later than the root player, so the root player would be losing slightly
            if (difference > 1) return -0.5;

            throw new ArgumentOutOfRangeException($"Evaluation values do not fall into the expected range: oTD={oTD:F3} | rTD={rTD:F3}");
        }

        /// <summary>
        /// Returns the value of the argument state with respect to the argument node.
        /// </summary>
        /// <param name="context">The context of the search.</param>
        /// <param name="node">The node that provides the context to evaluate the state.</param>
        /// <param name="state">The state that should be evaluated.</param>
        /// <returns>Double representing the value of the state with respect to the node.</returns>
        [Obsolete("EvaluateOld is deprecated and replaced by Evaluate.")]
        public double EvaluateOld(SearchContext<List<SabberStoneAction>, SabberStoneState, SabberStoneAction, object, SabberStoneAction> context, TreeSearchNode<SabberStoneState, SabberStoneAction> node, SabberStoneState state) {

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
            //var playerMinions = rootPlayer.BoardZone.GetAll();
            //var playerMinionsAttack = playerMinions.Where(i => i.CanAttack).Sum(j => j.AttackDamage);
            //var playerMinionsTauntHealth = playerMinions.Where(i => i.HasTaunt).Sum(j => j.Health);
            //var opponentMinions = opponent.BoardZone.GetAll();
            //var opponentMinionsAttack = opponentMinions.Where(i => i.CanAttack).Sum(j => j.AttackDamage);
            //var opponentMinionsTauntHealth = opponentMinions.Where(i => i.HasTaunt).Sum(j => j.Health);

            // Get some stats about player's resources
            //var playerCardsInHand = rootPlayer.HandZone.Count;
            //var opponentCardsInHand = opponent.HandZone.Count;
            var playerHealth = rootPlayer.Hero.Health;
            var opponentHealth = opponent.Hero.Health;
            //var playerCardsInDeck = rootPlayer.DeckZone.Count;
            //var opponentCardsInDeck = opponent.DeckZone.Count;
            //var playerWeaponDamage = rootPlayer.Hero.Weapon?.AttackDamage * rootPlayer.Hero.Weapon?.Durability ?? 0;
            //var opponentWeaponDamage = opponent.Hero.Weapon?.AttackDamage * opponent.Hero.Weapon?.Durability ?? 0;

            // Now use these stats to come to a number....
            //TODO implement Hearthstone state evaluation
            return playerHealth == opponentHealth ? 0 : playerHealth > opponentHealth ? .5 : -.5;
        }

        #endregion

    }
}
