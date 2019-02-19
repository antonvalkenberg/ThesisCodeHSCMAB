using AVThesis.Datastructures;
using AVThesis.Search;
using AVThesis.Search.Tree;
using Microsoft.VisualStudio.TestTools.UnitTesting;

/// <summary>
/// Written by A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by G.J. Roelofs and T. Aliyev.
/// </summary>
namespace AVThesisTest.TicTacToe {

    /// <summary>
    /// Tests search techniques in the Tic Tac Toe game.
    /// </summary>
    public class TicTacToeSearchTest : SearchTest<object, TicTacToeState, TicTacToeMove, object, OddmentTable<int>> {

        public AgentPlayout<object, TicTacToeState, TicTacToeMove, object, TicTacToeMove> PlayoutStrategy { get; set; }

        public WinLossDrawStateEvaluation<object, TicTacToeState, TicTacToeMove, object, TicTacToeMove, TreeSearchNode<TicTacToeState, TicTacToeMove>> EvaluationStrategy { get; set; }

        public void Setup() {
            // Setup
            State = new TicTacToeState();
            Agent = new TicTacToeGameLogic();
            GameLogic = new TicTacToeGameLogic();

            PlayoutStrategy = new AgentPlayout<object, TicTacToeState, TicTacToeMove, object, TicTacToeMove>(Agent);
            EvaluationStrategy = new WinLossDrawStateEvaluation<object, TicTacToeState, TicTacToeMove, object, TicTacToeMove, TreeSearchNode<TicTacToeState, TicTacToeMove>>(1, -10, 0);
        }

        public override void TestAI(SearchContext<object, TicTacToeState, TicTacToeMove, object, TicTacToeMove> context) {

            // Test if the technique can find the winning move.
            var source = new TicTacToeState("X-O-XO---");
            context.Reset();
            context.Source = source.Copy();
            var result = PlayGame(context);
            Assert.IsTrue(result.PlayerWon == 0);

            // Test if the technique can avoid a loss.
            source = new TicTacToeState("--X-OX---");
            source.EndTurn();
            context.Reset();
            context.Source = source.Copy();
            result = PlayGame(context);
            Assert.IsTrue(result.PlayerWon == -1);

            // On an empty board the game should be a draw.
            // (first player should play a corner position and second player should force the draw by playing middle)
            source = new TicTacToeState();
            context.Reset();
            context.Source = source.Copy();
            result = PlayGame(context);
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
