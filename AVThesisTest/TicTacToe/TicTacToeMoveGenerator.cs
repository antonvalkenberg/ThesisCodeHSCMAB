using System;
using System.Collections;
using System.Collections.Generic;
using AVThesis.Datastructures;

/// <summary>
/// Written by A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by G.J. Roelofs and T. Aliyev.
/// </summary>
namespace AVThesisTest.TicTacToe {

    public sealed class TicTacToeMoveGenerator : IPositionGenerator<TicTacToeMove> {
        public string Board { get; set; }

        public int PlayerID { get; set; }

        public int Position { get; set; } = -1;

        public TicTacToeMoveGenerator(TicTacToeState state) {
            // Initialise the position to the first open position on the board.
            Board = state.State;
            PlayerID = state.ActivePlayerID;
        }

        public static List<int> AllEmptyPositions(TicTacToeState state) {
            List<int> positions = new List<int>();

            char[] board = state.State.ToCharArray();
            int index = 0;
            while (index < board.Length) {
                if (board[index] == TicTacToeState.OPEN_SPACE) positions.Add(index);
                index++;
            }

            return positions;
        }

        private static int NextAvailablePosition(string board, int currentPosition) {
            // Check if the next position is available.
            int nextPosition = currentPosition;
            nextPosition++;
            while (board.Length > nextPosition && board.ToCharArray()[nextPosition] != TicTacToeState.OPEN_SPACE) {
                // If not, move one forward.
                nextPosition++;
            }
            // Return the next available position.
            return nextPosition;
        }

        #region IPositionGenerator
        
        public bool HasNext() {
            return NextAvailablePosition(Board, Current.PositionToPlace) < Board.Length;
        }

        #endregion

        #region IEnumerator

        public TicTacToeMove Current {
            get {
                if (Position >= 0 && Position < Board.Length) return new TicTacToeMove(Position, PlayerID);
                else throw new InvalidOperationException();
            }
        }

        object IEnumerator.Current => Current;

        public void Dispose() {
        }

        #endregion
        
        #region IEnumerable

        public IEnumerator<TicTacToeMove> GetEnumerator() {
            return this;
        }

        public bool MoveNext() {
            // Move to the next available position.
            Position = NextAvailablePosition(Board, Position);
            // Return whether or not that position is still on the board.
            return Position < Board.Length;
        }

        public void Reset() {
            Position = -1;
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return this;
        }

        #endregion
    }

}
