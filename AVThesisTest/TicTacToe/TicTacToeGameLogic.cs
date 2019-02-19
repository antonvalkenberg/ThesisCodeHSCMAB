using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AVThesis;
using AVThesis.Agent;
using AVThesis.Datastructures;
using AVThesis.Game;
using AVThesis.Search;
using AVThesis.Search.LSI;
using AVThesis.Search.Tree;

/// <summary>
/// Written by A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by G.J. Roelofs and T. Aliyev.
/// </summary>
namespace AVThesisTest.TicTacToe {

    public class TicTacToeGameLogic : IGameLogic<object, TicTacToeState, TicTacToeMove, object, TicTacToeMove, TicTacToeMove>, IAgent<SearchContext<object, TicTacToeState, TicTacToeMove, object, TicTacToeMove>, TicTacToeState, TicTacToeMove> {

        /// <summary>
        /// The amount of columns in the game of TicTacToe (3).
        /// </summary>
        public const int TICTACTOE_COLUMNS = 3;
        /// <summary>
        /// The amount of rows in the game of TicTacToe (3).
        /// </summary>
        public const int TICTACTOE_ROWS = 3;

        public static readonly List<int> CornerPositions = new List<int> {0, 2, 6, 8};
        public static readonly List<int> EdgePositions = new List<int> { 1, 3, 5, 7 };

        private static readonly Random rng = new Random();

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
            UpdateState(position);
            return position.Done;
        }

        public IPositionGenerator<TicTacToeMove> Expand(SearchContext<object, TicTacToeState, TicTacToeMove, object, TicTacToeMove> context, TicTacToeState position) {
            return new TicTacToeMoveGenerator(position);
        }

        public double[] Scores(TicTacToeState position) {
            return new double[] { 1 - position.PlayerWon, position.PlayerWon };
        }

        /// <inheritdoc />
        /// <summary>
        /// See <see cref="https://en.wikipedia.org/wiki/Tic-tac-toe#Strategy"/>
        /// </summary>
        public TicTacToeMove Act(SearchContext<object, TicTacToeState, TicTacToeMove, object, TicTacToeMove> context, TicTacToeState state) {
            var possibilities = TicTacToeMoveGenerator.AllEmptyPositions(state);
            var myID = state.ActivePlayerID;
            var oppID = TicTacToeState.SwitchPlayerID(myID);
            var oppMove = oppID == TicTacToeState.PLAYER_ONE_ID ? TicTacToeState.PLAYER_ONE_MOVE : TicTacToeState.PLAYER_TWO_MOVE;
            var openCorners = possibilities.Where(i => CornerPositions.Contains(i)).ToList();
            var openEdges = possibilities.Where(i => EdgePositions.Contains(i)).ToList();

            #region Opening and response

            // Opening move to corner or middle position
            // Debatable which is better, but we assume perfect play, so any corner is good
            if (possibilities.Count == 9) {
                return new TicTacToeMove(CornerPositions.RandomElementOrDefault(), myID);
                //return new TicTacToeMove(4, myID);
            }

            // If the middle position was opened with, play corner
            if (possibilities.Count == 8 && !possibilities.Contains(4)) {
                return new TicTacToeMove(CornerPositions.RandomElementOrDefault(), myID);
            }

            // If a corner or an edge was opened with, play middle
            if (possibilities.Count == 8)
                return new TicTacToeMove(4, myID);

            #endregion

            #region 1. Win

            // Check for own winning moves
            var myWins = CheckWinningPositions(state, myID);
            // Take the win
            if (!myWins.IsNullOrEmpty()) return new TicTacToeMove(myWins.RandomElementOrDefault(), myID);

            #endregion

            #region 2. Block

            // Check for opponent's winning moves
            var oppWins = CheckWinningPositions(state, oppID);
            // Prevent the loss
            if (!oppWins.IsNullOrEmpty()) return new TicTacToeMove(oppWins.RandomElementOrDefault(), myID);

            #endregion

            #region 3. Fork

            // Check if we have any forks available
            var forks = CheckForks(state, myID);
            // Move to one of the available forks
            if (!forks.IsNullOrEmpty()) return new TicTacToeMove(forks.RandomElementOrDefault(), myID);

            #endregion

            #region 4. Blocking an opponent's fork

            // Check if the opponent has any forks available
            var oppForks = CheckForks(state, oppID);
            if (!oppForks.IsNullOrEmpty()) {
                // If there is only one possible fork for the opponent, the player should block it.
                if (oppForks.Count == 1)
                    return new TicTacToeMove(oppForks[0], myID);
                // Otherwise, the player should block any forks in any way that simultaneously allows them to create two in a row, as long as it doesn't result in them creating a fork.
                var threats = CheckThreats(state, myID);
                var safeThreats = threats.Where(i => !DoesThisThreatCreateAForkForOpponent(state, i, myID)).ToList();
                var safeBlockingThreats = safeThreats.Where(i => oppForks.Contains(i)).ToList();
                if (!safeBlockingThreats.IsNullOrEmpty())
                    return new TicTacToeMove(safeBlockingThreats.RandomElementOrDefault(), myID);
                // Otherwise, the player should create a two in a row to force the opponent into defending, as long as it doesn't result in them creating a fork.
                if (!safeThreats.IsNullOrEmpty())
                    return new TicTacToeMove(safeThreats.RandomElementOrDefault(), myID);
            }

            #endregion

            #region 5. Center

            // If middle is open, play it
            if (possibilities.Contains(4))
                return new TicTacToeMove(4, myID);

            #endregion

            #region 6. Opposite corner

            // If the opponent is in a corner and the opposite corner is available, move there
            foreach (var cornerPosition in CornerPositions) {
                var oppositeCorner = OppositeCorner(cornerPosition);
                if (state.State[cornerPosition] == oppMove && openCorners.Contains(oppositeCorner))
                    return new TicTacToeMove(oppositeCorner, myID);
            }

            #endregion

            #region 7. Empty corner

            // If a corner is open, play it
            if (!openCorners.IsNullOrEmpty())
                return new TicTacToeMove(openCorners.RandomElementOrDefault(), myID);

            #endregion

            #region 8. Empty side

            // If an edge is open, play it
            if (!openEdges.IsNullOrEmpty())
                return new TicTacToeMove(openEdges.RandomElementOrDefault(), myID);

            #endregion

            // Otherwise, act random
            int index = rng.Next(possibilities.Count);
            int randomPosition = possibilities.ToArray()[index];
            // Return a random position to play for the active player
            return new TicTacToeMove(randomPosition, myID);
        }

        public List<int> CheckWinningPositions(TicTacToeState state, int playerID) {
            var possibilities = TicTacToeMoveGenerator.AllEmptyPositions(state);
            var wins = new List<int>();

            foreach (var possibility in possibilities) {
                var testMove = new TicTacToeMove(possibility, playerID);
                var clone = (TicTacToeState)state.Copy();
                clone = Apply(null, clone, testMove);
                if (clone.Done && clone.PlayerWon == playerID)
                    wins.Add(possibility);
            }

            return wins;
        }

        public List<int> CheckForks(TicTacToeState state, int playerID) {
            var possibilities = TicTacToeMoveGenerator.AllEmptyPositions(state);
            var forks = new List<int>();

            foreach (var possibility in possibilities) {
                var testMove = new TicTacToeMove(possibility, playerID);
                var clone = (TicTacToeState) state.Copy();
                clone = Apply(null, clone, testMove);
                var wins = CheckWinningPositions(clone, playerID);
                if (wins.Count >= 2)
                    forks.Add(possibility);
            }

            return forks;
        }

        public List<int> CheckThreats(TicTacToeState state, int playerID) {
            var possibilities = TicTacToeMoveGenerator.AllEmptyPositions(state);
            var threats = new List<int>();

            foreach (var possibility in possibilities) {
                var testMove = new TicTacToeMove(possibility, playerID);
                var clone = (TicTacToeState)state.Copy();
                clone = Apply(null, clone, testMove);
                if (!CheckWinningPositions(clone, playerID).IsNullOrEmpty())
                    threats.Add(possibility);
            }

            return threats;
        }

        public bool DoesThisThreatCreateAForkForOpponent(TicTacToeState state, int position, int playerID) {
            var clone = (TicTacToeState)state.Copy();
            clone = Apply(null, clone, new TicTacToeMove(position, playerID));
            // This move should be a threat, so see how the opponent must block
            var blocks = CheckWinningPositions(clone, playerID);
            clone = Apply(null, clone, new TicTacToeMove(blocks[0], TicTacToeState.SwitchPlayerID(playerID)));
            // A fork will be created for the opponent if this block gives them 2 or more winning positions
            return CheckWinningPositions(clone, TicTacToeState.SwitchPlayerID(playerID)).Count >= 2;
        }

        public int OppositeCorner(int cornerPosition) {
            switch (cornerPosition) {
                case 0: return 8;
                case 2: return 6;
                case 6: return 2;
                case 8: return 0;
                default: throw new ArgumentException($"Unknown corner position -> {cornerPosition}");
            }
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

        public class RandomTicTacToeMoveSampler : ISamplingStrategy<TicTacToeState, TicTacToeMove> {

            public TicTacToeMove Sample(TicTacToeState state) {
                return new TicTacToeMove(TicTacToeMoveGenerator.AllEmptyPositions(state).RandomElementOrDefault(), state.ActivePlayerID); 
            }

        }

        public class LSITicTacToeMoveSampler : ILSISamplingStrategy<TicTacToeState, TicTacToeMove, OddmentTable<int>> {

            public TicTacToeMove Sample(TicTacToeState state) {
                return null;
            }

            public TicTacToeMove Sample(TicTacToeState state, OddmentTable<int> sideInformation) {

                // Get all available positions in this state
                var emptyPositions = TicTacToeMoveGenerator.AllEmptyPositions(state);

                var positionSelected = false;
                var selectedPosition = -1;
                while (!positionSelected) {
                    // Sample a position from the OddmentTable
                    selectedPosition = sideInformation.Next();
                    // Check if this position can be played, otherwise generate a new one
                    positionSelected = emptyPositions.Contains(selectedPosition);
                }

                return new TicTacToeMove(selectedPosition, state.ActivePlayerID);
            }

        }

        public class LSITicTacToeSideInformation : ISideInformationStrategy<object, TicTacToeState, TicTacToeMove, object, TicTacToeMove, OddmentTable<int>> {

            public IGameLogic<object, TicTacToeState, TicTacToeMove, object, TicTacToeMove, TicTacToeMove> GameLogic { get; set; }

            public IPlayoutStrategy<object, TicTacToeState, TicTacToeMove, object, TicTacToeMove> PlayoutStrategy { get; set; }

            public IStateEvaluation<object, TicTacToeState, TicTacToeMove, object, TicTacToeMove, TreeSearchNode<TicTacToeState, TicTacToeMove>> EvaluationStrategy { get; set; }

            public void Setup(IGameLogic<object, TicTacToeState, TicTacToeMove, object, TicTacToeMove, TicTacToeMove> gameLogic,
                IPlayoutStrategy<object, TicTacToeState, TicTacToeMove, object, TicTacToeMove> playoutStrategy,
                IStateEvaluation<object, TicTacToeState, TicTacToeMove, object, TicTacToeMove, TreeSearchNode<TicTacToeState, TicTacToeMove>> evaluation) {
                GameLogic = gameLogic;
                PlayoutStrategy = playoutStrategy;
                EvaluationStrategy = evaluation;
            }

            public OddmentTable<int> Create(SearchContext<object, TicTacToeState, TicTacToeMove, object, TicTacToeMove> context, int samplesForGeneration) {
                
                var table = new Dictionary<int, double>();

                for (int i = 0; i < samplesForGeneration; i++) {
                    var action = new TicTacToeMove(TicTacToeMoveGenerator.AllEmptyPositions(context.Source).RandomElementOrDefault(), context.Source.ActivePlayerID);
                    var newState = GameLogic.Apply(context, (TicTacToeState) context.Source.Copy(), action);
                    var endState = PlayoutStrategy.Playout(context, newState);
                    var value = EvaluationStrategy.Evaluate(context, new TreeSearchNode<TicTacToeState, TicTacToeMove>(action), endState);
                    if (!table.ContainsKey(action.PositionToPlace)) table.Add(action.PositionToPlace, 0);
                    table[action.PositionToPlace] += value;
                }

                var maxValue = table.Values.Max();
                var minValue = table.Values.Min();

                var oddmentTable = new OddmentTable<int>();
                foreach (var kvPair in table) {
                    var normalisedValue = Util.Normalise(kvPair.Value, minValue, maxValue);
                    oddmentTable.Add(kvPair.Key, normalisedValue, recalculate: false);
                }
                oddmentTable.Recalculate();

                return oddmentTable;
            }

        }

    }

}
