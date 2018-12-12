using AVThesis.Test;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AVThesisTest {

    [TestClass]
    public class SearchTechniquesTest {

        [TestMethod]
        public void TestMCTS() {
            var test = new TicTacToeSearchTest();
            test.Setup();
            test.TestMCTS();
        }

        [TestMethod]
        public void TestFlatMCS() {
            var test = new TicTacToeSearchTest();
            test.Setup();
            test.TestFlatMCS();
        }

        [TestMethod]
        public void TestNMCTS() {
            var test = new TicTacToeSearchTest();
            test.Setup();
            test.TestNMCTS();
        }

    }

}
