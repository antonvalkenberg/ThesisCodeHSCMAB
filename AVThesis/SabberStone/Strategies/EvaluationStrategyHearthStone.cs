using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AVThesis.SabberStone.Bots;
using AVThesis.Search;
using AVThesis.Search.Tree;

/// <summary>
/// Copyright © 2019 Anton Valkenberg
/// Written by BSc. A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by MSc. G.-J. Roelofs, MSc. T. Aliyev and MSc. D. de Rydt.
/// </summary>
namespace AVThesis.SabberStone.Strategies {

    /// <summary>
    /// Represents a way of evaluating a board state in HearthStone.
    /// </summary>
    public class EvaluationStrategyHearthStone : IStateEvaluation<List<SabberStoneAction>, SabberStoneState, SabberStoneAction, object, SabberStoneAction, TreeSearchNode<SabberStoneState, SabberStoneAction>> {
        
        #region Properties

        /// <summary>
        /// The heuristic agent that contains another evaluation function.
        /// </summary>
        public HeuristicBot HeuristicAgent { get; set; }

        /// <summary>
        /// Whether or not to use the heuristic agent's evaluation function.
        /// </summary>
        public bool UseHeuristicBotEvaluation { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of this state evaluation strategy.
        /// </summary>
        /// <param name="useHeuristicBotEvaluation">[Optional] Whether or not to use the heuristic agent's evaluation function. Default value is false.</param>
        public EvaluationStrategyHearthStone(bool useHeuristicBotEvaluation = false) {
            UseHeuristicBotEvaluation = useHeuristicBotEvaluation;
            if (useHeuristicBotEvaluation) HeuristicAgent = new HeuristicBot();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns the value of the argument state with respect to the argument node.
        /// </summary>
        /// <param name="context">The context of the search.</param>
        /// <param name="node">The node that provides the context to evaluate the state.</param>
        /// <param name="state">The state that should be evaluated.</param>
        /// <returns>Double representing the value of the state with respect to the node.</returns>
        public double Evaluate(SearchContext<List<SabberStoneAction>, SabberStoneState, SabberStoneAction, object, SabberStoneAction> context, TreeSearchNode<SabberStoneState, SabberStoneAction> node, SabberStoneState state) {

            // Check if we can and want to use the HeuristicBot's evaluation
            if (UseHeuristicBotEvaluation) {
                // This scoring function is actually used to score the effect of tasks, but we are using it here to score the effect of the transition from our Source state to the state from which we are currently evaluating.
                // TODO using the HeuristicBot's evaluation function could be improved
                var heuristicEvaluation = HeuristicAgent.EvaluateStateTransition(context.Source, state);
                // Colour the evaluation depending on who the active player is in the state
                var isRootPlayer = state.CurrentPlayer() == context.Source.CurrentPlayer();
                heuristicEvaluation = isRootPlayer ? heuristicEvaluation : heuristicEvaluation * -1;
                // Normalise the value between -1 and 1. The min and max values have been empirically set and equal the min and max possible evaluations that are returned by the HeuristicBot's function.
                var norm = 2 * Util.Normalise(heuristicEvaluation, -50, 50) - 1; // Note: this is a transformation from [0,1] to [-1,1]
                return norm;
            }

            var rootPlayerId = context.Source.CurrentPlayer();
            var rootPlayer = state.Player1.Id == rootPlayerId ? state.Player1 : state.Player2;
            var opponent = rootPlayer.Opponent;

            // Check for a win/loss
            if (state.PlayerWon != State.DRAW) {
                return state.PlayerWon == rootPlayerId ? 1 : -1;
            }

            // Gather stats that we need
            // TODO gather stats from cards in hand

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
            if (notARaceSituation) {
                // Two aspects here (to keep it simple)
                // -> root player's HP vs opponent's HP
                // -> root player's #creatures vs opponent's #creatures

                // Having more HP ánd more creatures is quite good
                if (rH > oH && rootPlayerMinions.Length > opponentMinions.Length)
                    return 0.75;
                if (rH > oH && rootPlayerMinions.Length == opponentMinions.Length)
                    return 0.25;
                if (rH > oH && rootPlayerMinions.Length < opponentMinions.Length)
                    return 0.1;

                if (rH == oH && rootPlayerMinions.Length > opponentMinions.Length)
                    return 0.33;
                if (rH == oH && rootPlayerMinions.Length == opponentMinions.Length)
                    return 0;
                if (rH == oH && rootPlayerMinions.Length < opponentMinions.Length)
                    return -0.33;

                if (rH < oH && rootPlayerMinions.Length > opponentMinions.Length)
                    return -0.1;
                if (rH < oH && rootPlayerMinions.Length == opponentMinions.Length)
                    return -0.25;
                // Having less HP ánd less creatures is quite bad
                if (rH < oH && rootPlayerMinions.Length < opponentMinions.Length)
                    return -0.75;
            }
            
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

        #endregion

    }
}
