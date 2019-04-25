using System.Collections.Generic;

/// <summary>
/// Copyright © 2019 Anton Valkenberg
/// Written by BSc. A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by MSc. G.-J. Roelofs, MSc. T. Aliyev and MSc. D. de Rydt.
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
