using AVThesis.Search;

/// <summary>
/// Written by A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by G.J. Roelofs and T. Aliyev.
/// </summary>
namespace AVThesis.Test {

    public class TicTacToeState : State {

        public const int PLAYER_ONE_ID = 0;
        public const int PLAYER_TWO_ID = 1;
        public const char PLAYER_ONE_MOVE = 'X';
        public const char PLAYER_TWO_MOVE = 'O';
        public const char OPEN_SPACE = '-';

        private string _state;
        private int _activePlayerID = 0;
        private bool _done = false;

        public string State { get => _state; set => _state = value; }
        public int ActivePlayerID { get => _activePlayerID; set => _activePlayerID = value; }
        public bool Done { get => _done; set => _done = value; }

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
            return playerID == PLAYER_ONE_ID ? PLAYER_TWO_ID : PLAYER_ONE_ID;
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

        public override long HashMethod() {
            return State.GetHashCode();
        }

        public override int NumberOfPlayers() {
            return 2;
        }

    }

}
