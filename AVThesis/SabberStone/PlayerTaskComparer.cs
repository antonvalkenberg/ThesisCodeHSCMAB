using System.Collections.Generic;

/// <summary>
/// Written by A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by G.J. Roelofs and T. Aliyev.
/// </summary>
namespace AVThesis.SabberStone {

    /// <summary>
    /// EqualityComparer for <see cref="SabberStonePlayerTask"/>.
    /// </summary>
    public class PlayerTaskComparer : IEqualityComparer<SabberStonePlayerTask> {

        public static readonly PlayerTaskComparer Comparer = new PlayerTaskComparer();

        public bool Equals(SabberStonePlayerTask x, SabberStonePlayerTask y) {
            if (x == null || y == null) return false;
            if (ReferenceEquals(x, y)) return true;
            return x.GetHashCode() == y.GetHashCode();
        }

        public int GetHashCode(SabberStonePlayerTask obj) {
            return obj.GetHashCode();
        }

    }

}
