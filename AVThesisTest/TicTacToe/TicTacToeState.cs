using System;
using AVThesis.Search;
using Constants = AVThesis.Constants;

/// <summary>
/// Copyright © 2019 Anton Valkenberg
/// Written by BSc. A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by MSc. G.-J. Roelofs, MSc. T. Aliyev and MSc. D. de Rydt.
/// </summary>
namespace AVThesisTest.TicTacToe {

    public class TicTacToeState : State {

        public const int PLAYER_ONE_ID = 0;
        public const int PLAYER_TWO_ID = 1;
        public const char PLAYER_ONE_MOVE = 'X';
        public const char PLAYER_TWO_MOVE = 'O';
        public const char OPEN_SPACE = '-';

        public string State { get; set; }
        public int ActivePlayerID { get; set; }
        public bool Done { get; set; }

        public TicTacToeState() {
            State = new string(OPEN_SPACE, 9);
        }

        public TicTacToeState(string board) {
            State = new string(board.ToCharArray());
        }

        public TicTacToeState(string state, int activePlayerID, bool done) {
            State = new string(state.ToCharArray());
            ActivePlayerID = activePlayerID;
            Done = done;
        }

        public void EndTurn() {
            ActivePlayerID = SwitchPlayerID(ActivePlayerID);
        }

        public static int SwitchPlayerID(int playerID) {
            return Math.Abs(playerID-1);
        }

        public override string ToString() {
            return string.Format("[Active Player ID: {1} | Done: {2} | Board: {0}{3}{0}{4}{0}{5}]", "\r\n", ActivePlayerID, Done, State.Substring(0, 3), State.Substring(3, 3), State.Substring(6, 3));
        }

        public override dynamic Copy() {
            return new TicTacToeState(State, ActivePlayerID, Done);
        }

        public override int CurrentPlayer() {
            return ActivePlayerID;
        }

        public override bool Equals(State otherState) {
            return otherState is TicTacToeState state && Equals(state);
        }

        public bool Equals(TicTacToeState otherState) {
            return HashMethod() == otherState.HashMethod();
        }

        public override int HashMethod() {
            unchecked { // overflow is fine, the number just wraps
                var hash = (int)Constants.HASH_OFFSET_BASIS;
                hash = Constants.HASH_FNV_PRIME * (hash ^ State.GetHashCode());
                hash = Constants.HASH_FNV_PRIME * (hash ^ (ActivePlayerID + 1));
                return hash;
            }
        }

        public override int NumberOfPlayers() {
            return 2;
        }

        public override bool IsTerminal() {
            return Done;
        }

    }

}
