using AVThesis.Search;
using Microsoft.VisualStudio.TestTools.UnitTesting;

/// <summary>
/// Written by A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by G.J. Roelofs and T. Aliyev.
/// </summary>
namespace AVThesis.Test {

    /// <summary>
    /// Tests search techniques in the Tic Tac Toe game.
    /// </summary>
    public class TicTacToeSearchTest : SearchTest<object, TicTacToeState, TicTacToeMove, object> {
        
        public void Setup() {
            // Setup
            State = new TicTacToeState();
            Agent = new TicTacToeGameLogic();
            GameLogic = new TicTacToeGameLogic();
        }

        public override void TestAI(SearchContext<object, TicTacToeState, TicTacToeMove, object, TicTacToeMove> context) {

            // On an empty board the game should be a draw.
            // (first player should play a corner position and second player should force the draw by playing middle)
            var source = new TicTacToeState();

            context.Reset();
            context.Source = source.Copy();

            var result = PlayGame(context);

            Assert.IsTrue(result.PlayerWon == -1);

            // If the first player plays the middle position, the game is a draw.
            // (second player should play a corner position)
            source = new TicTacToeState("----X----");
            source.EndTurn();

            context.Reset();
            context.Source = source.Copy();

            result = PlayGame(context);

            Assert.IsTrue(result.PlayerWon == -1);
            
            // If the first player plays an edge position, the game is a draw.
            // (second player should play a corner position)
            source = new TicTacToeState("---X-----");
            source.EndTurn();

            context.Reset();
            context.Source = source.Copy();

            result = PlayGame(context);

            Assert.IsTrue(result.PlayerWon == -1);
        }

    }
}
