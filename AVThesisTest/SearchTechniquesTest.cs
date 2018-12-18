using AVThesisTest.TicTacToe;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AVThesisTest {

    [TestClass]
    public class SearchTechniquesTest {

        [TestMethod]
        public void TestFlatMCSTicTacToe() {
            var test = new TicTacToeSearchTest();
            test.Setup();
            test.TestFlatMCS();
        }

        [TestMethod]
        public void TestMCTSTicTacToe() {
            var test = new TicTacToeSearchTest();
            test.Setup();
            test.TestMCTS();
        }

        [TestMethod]
        public void TestNMCTSTicTacToe() {
            var test = new TicTacToeSearchTest();
            test.Setup();
            var samplingStrategy = new TicTacToeGameLogic.RandomTicTacToeMoveSampler();
            test.TestNMCTS(samplingStrategy);
        }

        [TestMethod]
        public void TestLSITicTacToe() {
            var test = new TicTacToeSearchTest();
            test.Setup();
            var sideInformationStrategy = new TicTacToeGameLogic.LSITicTacToeSideInformation();
            sideInformationStrategy.Setup(test.GameLogic, test.PlayoutStrategy, test.EvaluationStrategy);
            var samplingStrategy = new TicTacToeGameLogic.LSITicTacToeMoveSampler();
            test.TestLSI(sideInformationStrategy, samplingStrategy);
        }

    }

}
