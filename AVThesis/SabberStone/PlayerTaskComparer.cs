using System.Collections.Generic;
using SabberStoneCore.Tasks;

/// <summary>
/// Written by A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by G.J. Roelofs and T. Aliyev.
/// </summary>
namespace AVThesis.SabberStone {

    /// <summary>
    /// EqualityComparer for <see cref="PlayerTask"/>.
    /// </summary>
    public class PlayerTaskComparer : IEqualityComparer<PlayerTask> {

        public bool Equals(PlayerTask x, PlayerTask y) {
            if (x == null || y == null) return false;
            if (ReferenceEquals(x, y)) return true;
            return x.GetHashCode() == y.GetHashCode();
        }

        public int GetHashCode(PlayerTask obj) {
            //TODO I don't like relying on the FullPrint method to generate a HashCode for PlayerTasks, potentially implement GetHashCode ourselves.
            return obj.FullPrint().GetHashCode();
        }

    }

}
