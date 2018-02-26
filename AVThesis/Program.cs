using System;
using AVThesis.SabberStone;
using SabberStoneCore.Config;
using SabberStoneCore.Enums;
using AVThesis.Bots;

/// <summary>
/// Written by A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by G.J. Roelofs and T. Aliyev.
/// </summary>
namespace AVThesis {

    class Program {

				static void Main(string[] args) {
            RunTournamentMatch();

            string catcher = null;
        }

        public static void RunTournamentMatch() {
            // Configure the tournament game structure
            var gameConfig = new GameConfig {
                Player1Name = "Player1",
                Player1HeroClass = CardClass.HUNTER,
                Player1Deck = Decks.TestDeck,
                Player2Name = "Player2",
                Player2HeroClass = CardClass.HUNTER,
                Player2Deck = Decks.TestDeck,
                FillDecks = false,
                Shuffle = true,
                SkipMulligan = true,
                History = false
            };

            // Create a new tournament match
            var match = new Tournament.TournamentMatch(new RandomBot(), new RandomBot(), gameConfig, 2, printToConsole: true);

            match.RunMatch();
        }

        public static void RunQuickMatch() {

            var game = new SabberStoneState(new SabberStoneCore.Model.Game(new GameConfig {
                StartPlayer = 1,
                Player1Name = "Player1",
                Player1HeroClass = CardClass.HUNTER,
                Player1Deck = Decks.TestDeck,
                Player2Name = "Player2",
                Player2HeroClass = CardClass.HUNTER,
                Player2Deck = Decks.TestDeck,
                FillDecks = false,
                Shuffle = true,
                SkipMulligan = true,
                History = true
            }));

            // Create two bots to play
            var bot1 = new RandomBot(game.Player1);
            var bot2 = new RandomBot(game.Player2);


            game.Game.StartGame();

            // Mulligan stuff can happen in between here.

            game.Game.MainReady();

            while (game.Game.State != State.COMPLETE) {
                // Some info
                Console.WriteLine("");
                Console.WriteLine($"Player1: {game.Player1.PlayState} / Player2: {game.Player2.PlayState} - " +
                          $"ROUND {(game.Game.Turn + 1) / 2} - {game.Game.CurrentPlayer.Name}");
                Console.WriteLine($"Hero[P1]: {game.Player1.Hero.Health} / Hero[P2]: {game.Player2.Hero.Health}");
                Console.WriteLine("");

                // Handle all of player 1's turn
                Console.WriteLine($"- Player 1 - <{game.Game.CurrentPlayer.Name}> ---------------------------");
                while (game.Game.State == State.RUNNING && game.Game.CurrentPlayer == game.Player1) {

                    // Ask the bot to act.
                    var action = bot1.Act(game);

                    // Stop if there is nothing more to do
                    if (action == null) break;

                    // Process the task
                    Console.WriteLine(action.Action.FullPrint());
                    game.Game.Process(action.Action);
                }

                // Handle all of player 2's turn
                Console.WriteLine($"- Player 2 - <{game.Game.CurrentPlayer.Name}> ---------------------------");
                while (game.Game.State == State.RUNNING && game.Game.CurrentPlayer == game.Player2) {

                    // Ask the bot to act.
                    var action = bot2.Act(game);

                    // Stop if there is nothing more to do
                    if (action == null) break;

                    // Process the task
                    Console.WriteLine(action.Action.FullPrint());
                    game.Game.Process(action.Action);
                }
            }

            Console.WriteLine($"Game: {game.Game.State}, Player1: {game.Player1.PlayState} / Player2: {game.Player2.PlayState}");
        }

        public static void RunTicTacToeTest() {
            var test = new Test.TicTacToeSearchTest();
            test.Setup();
            test.TestMCTS();
        }

    }

}
