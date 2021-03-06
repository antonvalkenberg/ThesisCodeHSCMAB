﻿using System.Collections.Generic;
using AVThesis.Search;

/// <summary>
/// Copyright © 2019 Anton Valkenberg
/// Written by BSc. A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by MSc. G.-J. Roelofs, MSc. T. Aliyev and MSc. D. de Rydt.
/// </summary>
namespace AVThesis.SabberStone.Strategies {

    /// <summary>
    /// Goal strategy that cuts off after a set amount of turns.
    /// </summary>
    public class GoalStrategyTurnCutoff : IGoalStrategy<List<SabberStoneAction>, SabberStoneState, SabberStoneAction, object, SabberStoneAction> {

        #region Properties

        /// <summary>
        /// The amount of turns after which this goal strategy cuts off.
        /// </summary>
        public int CutoffThreshold { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Create a new instance with a set cutoff threshold.
        /// Note: the cutoff is based on turns as tracked by <see cref="SabberStoneCore.Model.Game.Turn"/>.
        /// </summary>
        /// <param name="cutoffThreshold">The amount of turns after which the cutoff happens.</param>
        public GoalStrategyTurnCutoff(int cutoffThreshold) {
            CutoffThreshold = cutoffThreshold;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Determines if a Position represents a completed search.
        /// </summary>
        /// <param name="context">The context of the search.</param>
        /// <param name="position">The Position.</param>
        /// <returns>Whether or not the search is done.</returns>
        public bool Done(SearchContext<List<SabberStoneAction>, SabberStoneState, SabberStoneAction, object, SabberStoneAction> context, SabberStoneState position) {

            // Determine the turn in which the search started.
            var sourceTurn = context.Source.Game.Turn;

            // The goal will be reached if the amount of turns since the source turn is greater than or equal to the cutoff, or the game has been completed.
            return position.Game.Turn - sourceTurn >= CutoffThreshold || position.Game.State == SabberStoneCore.Enums.State.COMPLETE;
        }

        #endregion

    }

}
