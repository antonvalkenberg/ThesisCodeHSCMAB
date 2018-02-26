using AVThesis.Game;

/// <summary>
/// Written by A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by G.J. Roelofs and T. Aliyev.
/// </summary>
namespace AVThesis.Test {

    public class TicTacToeMove : IMove {

        private int _positionToPlace;
        private int _playerID;

        public int PositionToPlace { get => _positionToPlace; set => _positionToPlace = value; }
        public int PlayerID { get => _playerID; set => _playerID = value; }

        public TicTacToeMove(int positionToPlace, int playerID) {
            PositionToPlace = positionToPlace;
            PlayerID = playerID;
        }

        public int Player() {
            return PlayerID;
        }

        public override string ToString() {
            return string.Format("[Position: {0} | PlayerID: {1}]", PositionToPlace, PlayerID);
        }

    }
}
