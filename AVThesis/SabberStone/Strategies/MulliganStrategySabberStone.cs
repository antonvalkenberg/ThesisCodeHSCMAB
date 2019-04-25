using System.Linq;
using SabberStoneCore.Model.Entities;
using SabberStoneCore.Tasks.PlayerTasks;

/// <summary>
/// Copyright © 2019 Anton Valkenberg
/// Written by BSc. A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by MSc. G.-J. Roelofs, MSc. T. Aliyev and MSc. D. de Rydt.
/// </summary>
namespace AVThesis.SabberStone.Strategies {

    /// <summary>
    /// Defines mulligan strategies for a game of SabberStone.
    /// </summary>
    public static class MulliganStrategySabberStone {

        /// <summary>
        /// The default cost threshold for mulligans.
        /// </summary>
        public const int DEFAULT_MULLIGAN_COST_THRESHOLD = 3;

        /// <summary>
        /// Mulligans any card that has a cost higher than <see cref="DEFAULT_MULLIGAN_COST_THRESHOLD"/>.
        /// ALERT: This <see cref="ChooseTask"/> is not very intuitive because you have to choose which cards to KEEP, not which ones to mulligan.
        /// </summary>
        /// <param name="player">The player that is allowed to take a mulligan.</param>
        /// <returns><see cref="ChooseTask"/> containing the unique identifiers of the cards in the player's hand that have been chosen to be KEPT.</returns>
        public static ChooseTask DefaultMulligan(Controller player) {
            return ChooseTask.Mulligan(player, player.HandZone.Where(i => i.Cost <= DEFAULT_MULLIGAN_COST_THRESHOLD).Select(i => i.Id).ToList());
        }

    }

}
