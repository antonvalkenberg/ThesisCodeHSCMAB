using System;
using AVThesis;
using AVThesis.Game;

/// <summary>
/// Written by A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by G.J. Roelofs and T. Aliyev.
/// </summary>
namespace AVThesisTest.TicTacToe {

    public class TicTacToeMove : IMove, IEquatable<TicTacToeMove> {

        public int PositionToPlace { get; }
        public int PlayerID { get; }

        public TicTacToeMove(int positionToPlace, int playerID) {
            PositionToPlace = positionToPlace;
            PlayerID = playerID;
        }

        public int Player() {
            return PlayerID;
        }

        public static bool operator ==(TicTacToeMove left, TicTacToeMove right) {
            return Equals(left, right);
        }

        public static bool operator !=(TicTacToeMove left, TicTacToeMove right) {
            return !Equals(left, right);
        }

        public bool Equals(TicTacToeMove other) {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return GetHashCode() == other.GetHashCode();
        }

        public override bool Equals(object obj) {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((TicTacToeMove)obj);
        }

        public override int GetHashCode() {
            unchecked { // overflow is fine, the number just wraps
                var hash = (int)Constants.HASH_OFFSET_BASIS;
                hash = Constants.HASH_FNV_PRIME * (hash ^ PositionToPlace);
                hash = Constants.HASH_FNV_PRIME * (hash ^ (PlayerID + 1));
                return hash;
            }
        }

        public override string ToString() {
            return $"[Position: {PositionToPlace} | PlayerID: {PlayerID}]";
        }

    }
}
