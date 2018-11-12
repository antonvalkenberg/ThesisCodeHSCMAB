using System.Collections.Generic;
using System.Text;
using AVThesis.Agent;
using AVThesis.Datastructures;
using AVThesis.Game;
using AVThesis.Search;

/// <summary>
/// Written by A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by G.J. Roelofs and T. Aliyev.
/// </summary>
namespace AVThesis.Test {

    public class TicTacToeGameLogic : IGameLogic<object, TicTacToeState, TicTacToeMove, object, TicTacToeMove, TicTacToeMove>, IAgent<SearchContext<object, TicTacToeState, TicTacToeMove, object, TicTacToeMove>, TicTacToeState, TicTacToeMove> {

        /// <summary>
        /// The amount of columns in the game of TicTacToe (3).
        /// </summary>
        public const int TICTACTOE_COLUMNS = 3;
        /// <summary>
        /// The amount of rows in the game of TicTacToe (3).
        /// </summary>
        public const int TICTACTOE_ROWS = 3;

        public TicTacToeState Apply(SearchContext<object, TicTacToeState, TicTacToeMove, object, TicTacToeMove> context, TicTacToeState position, TicTacToeMove action) {
            // Play the move in the argument action on the argument state
            StringBuilder newState = new StringBuilder(position.State);
            newState.Replace(TicTacToeState.OPEN_SPACE, action.PlayerID == TicTacToeState.PLAYER_ONE_ID ? TicTacToeState.PLAYER_ONE_MOVE : TicTacToeState.PLAYER_TWO_MOVE, action.PositionToPlace, 1);

            // Rollover the turn
            position.State = newState.ToString();
            position.EndTurn();

            // Update the state
            UpdateState(position);

            return position;
        }

        public bool Done(SearchContext<object, TicTacToeState, TicTacToeMove, object, TicTacToeMove> context, TicTacToeState position) {
            return position.Done;
        }

        public IPositionGenerator<TicTacToeMove> Expand(SearchContext<object, TicTacToeState, TicTacToeMove, object, TicTacToeMove> context, TicTacToeState position) {
            return new TicTacToeMoveGenerator(position);
        }

        public double[] Scores(TicTacToeState position) {
            return new double[] { 1 - position.PlayerWon, position.PlayerWon };
        }

        public TicTacToeMove Act(SearchContext<object, TicTacToeState, TicTacToeMove, object, TicTacToeMove> context, TicTacToeState state) {
            List<int> possibilities = TicTacToeMoveGenerator.AllEmptyPositions(state);

            // Check for own winning moves
            var myID = state.ActivePlayerID;
            foreach (var possibility in possibilities) {
                var testMove = new TicTacToeMove(possibility, myID);
                var clone = (TicTacToeState) state.Copy();
                clone = Apply(null, clone, testMove);
                if (clone.Done && clone.PlayerWon == myID)
                    return testMove;
            }

            // Check for opponent's winning moves
            var oppID = myID == TicTacToeState.PLAYER_ONE_ID
                ? TicTacToeState.PLAYER_ONE_ID
                : TicTacToeState.PLAYER_TWO_ID;
            foreach (var possibility in possibilities) {
                var testMove = new TicTacToeMove(possibility, oppID);
                var clone = (TicTacToeState)state.Copy();
                clone = Apply(null, clone, testMove);
                if (clone.Done && clone.PlayerWon == oppID)
                    return new TicTacToeMove(possibility, myID);
            }

            // Otherwise, act random
            int index = new System.Random().Next(possibilities.Count);
            int randomPosition = possibilities.ToArray()[index];

            // Return a random position to play for the active player
            return new TicTacToeMove(randomPosition, state.ActivePlayerID);
        }

        public void UpdateState(TicTacToeState state) {
            char[] board = state.State.ToCharArray();

            // First of possibilities is when we have three in a row.
            // There are three rows and for each row if all three elements are same and non empty then we have a winner.
            int playerWon = State.DRAW;
            for (int i = 0; i < TICTACTOE_ROWS; i++) {
                // Check for repetition of a symbol across the row.
                if (board[3 * i] != TicTacToeState.OPEN_SPACE
                    && board[3 * i] == board[3 * i + 1] 
                    && board[3 * i] == board[3 * i + 2]) {

                    // Check which symbol was repeated.
                    playerWon = board[3 * i] == TicTacToeState.PLAYER_ONE_MOVE ? 0 : 1;
                }
            }

            // Second possibilities are when we have columns of three in a row.
            if (playerWon == State.DRAW) {
                for (int i = 0; i < 3; i++) {
                    if (board[i] != TicTacToeState.OPEN_SPACE && board[i] == board[i + 3] && board[i] == board[i + 6]) {
                        if (board[i] == TicTacToeState.PLAYER_ONE_MOVE)
                            playerWon = 0;
                        else
                            playerWon = 1;
                    }
                }
            }

            // Last possibility: two diagonals; 0->4->8 or 2->4->6.
            if (playerWon == State.DRAW && board[4] != TicTacToeState.OPEN_SPACE) {
                if ((board[0] == board[8] && board[0] == board[4])
                  || (board[2] == board[6] && board[2] == board[4])) {
                    if (board[4] == TicTacToeState.PLAYER_ONE_MOVE)
                        playerWon = 0;
                    else
                        playerWon = 1;
                }
            }

            // Check if someone has won and update the state.
            if (playerWon != State.DRAW) {
                state.Done = true;
                state.PlayerWon = playerWon;
            }
            // Check if the board is full, which means we have a draw.
            else if (!state.State.Contains(TicTacToeState.OPEN_SPACE.ToString())) {
                state.Done = true;
                state.PlayerWon = State.DRAW;
            }
        }

    }

}
