using AVThesisTest.TicTacToe;
using Microsoft.VisualStudio.TestTools.UnitTesting;

/// <summary>
/// Copyright © 2019 Anton Valkenberg
/// Written by BSc. A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by MSc. G.-J. Roelofs, MSc. T. Aliyev and MSc. D. de Rydt.
/// </summary>
namespace AVThesisTest {

    [TestClass]
    public class SearchTechniquesTest {

        // The TicTacToe tests don't actually work very well, because the playout-bot does not play perfectly.

        [TestMethod]
        public void TestFlatMCSTicTacToe() {
            var test = new TicTacToeSearchTest();
            test.Setup();
            var search = test.SetupFlatMCS();
            test.TestFlatMCS(search);
        }

        [TestMethod]
        public void TestMCTSTicTacToe() {
            var test = new TicTacToeSearchTest();
            test.Setup();
            var search = test.SetupMCTS();
            test.TestMCTS(search);
        }

        [TestMethod]
        public void TestNMCTSTicTacToe() {
            var test = new TicTacToeSearchTest();
            test.Setup();
            var samplingStrategy = new TicTacToeGameLogic.RandomTicTacToeMoveSampler();
            var search = test.SetupNMCTS(samplingStrategy);
            test.TestNMCTS(search);
        }

        [TestMethod]
        public void TestLSITicTacToe() {
            var test = new TicTacToeSearchTest();
            test.Setup();
            var sideInformationStrategy = new TicTacToeGameLogic.LSITicTacToeSideInformation();
            sideInformationStrategy.Setup(test.GameLogic, test.PlayoutStrategy, test.EvaluationStrategy);
            var samplingStrategy = new TicTacToeGameLogic.LSITicTacToeMoveSampler();
            var search = test.SetupLSI(sideInformationStrategy, samplingStrategy);
            test.TestLSI(search);
        }

    }

}
