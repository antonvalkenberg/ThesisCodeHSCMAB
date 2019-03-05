using System;
using AVThesis.SabberStone;
using SabberStoneCore.Config;
using SabberStoneCore.Enums;
using AVThesis.SabberStone.Bots;

/// <summary>
/// Written by A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by G.J. Roelofs and T. Aliyev.
/// </summary>
namespace AVThesis {

    public class Program {

		public static void Main(string[] args) {
            //RunTournamentMatch();
            RunQuickMatch();

#pragma warning disable 219
            string catcher = null;
#pragma warning restore 219
        }

        public static void RunTournamentMatch() {
            // Configure the tournament game structure
            var gameConfig = new GameConfig {
                Player1Name = "Player1",
                Player1HeroClass = CardClass.HUNTER,
                Player1Deck = Decks.MidrangeHunter,
                Player2Name = "Player2",
                Player2HeroClass = CardClass.HUNTER,
                Player2Deck = Decks.MidrangeHunter,
                FillDecks = false,
                Shuffle = true,
                SkipMulligan = true,
                History = false
            };

            // Create a new tournament match
            var bot1 = new NMCTSBot();
            var bot2 = new LSIBot();
            var match = new Tournament.TournamentMatch(bot1, bot2, gameConfig, 5);

            match.RunMatch();
        }

        public static void RunQuickMatch() {

            var game = new SabberStoneState(new SabberStoneCore.Model.Game(new GameConfig {
                StartPlayer = 1,
                Player1Name = "Player1",
                Player1HeroClass = CardClass.HUNTER,
                Player1Deck = Decks.DefaultDeck,
                Player2Name = "Player2",
                Player2HeroClass = CardClass.HUNTER,
                Player2Deck = Decks.AggroHunter,
                FillDecks = false,
                Shuffle = true,
                SkipMulligan = true,
                History = false
            }));

            // Create two bots to play
            var bot1 = new MCTSBot(game.Player1);
            var bot2 = new RandomBot(game.Player2);

            game.Game.StartGame();

            // Mulligan stuff can happen in between here.
            
            while (game.Game.State != State.COMPLETE) {
                Console.WriteLine("");
                Console.WriteLine($"TURN {(game.Game.Turn + 1) / 2} - {game.Game.CurrentPlayer.Name}");
                Console.WriteLine($"Hero[P1] {game.Player1.Hero} HP: {game.Player1.Hero.Health} / Hero[P2] {game.Player2.Hero} HP: {game.Player2.Hero.Health}");

                // Check if the current player is Player1
                if (game.Game.CurrentPlayer.Id == game.Player1.Id) {
                    
                    // Ask the bot to act.
                    var action = bot1.Act(game);

                    Console.WriteLine($"- {game.Game.CurrentPlayer.Name} Action ----------------------------");

                    // Check if the action is valid
                    if (action != null && action.IsComplete()) {

                        // Process the tasks in the action
                        foreach (var item in action.Tasks) {

                            // Process the task
                            Console.WriteLine(item.Task.FullPrint());
                            game.Game.Process(item.Task);
                        }
                    }
                }

                // Check if Player1's action ended the game.
                if (game.Game.State == State.COMPLETE) break;
                Console.WriteLine("*");
                
                // Check if the current player is Player2
                if (game.Game.CurrentPlayer.Id == game.Player2.Id) {

                    // Ask the bot to act.
                    var action = bot2.Act(game);

                    Console.WriteLine($"- {game.Game.CurrentPlayer.Name} Action ----------------------------");

                    // Check if the action is valid
                    if (action != null && action.IsComplete()) {

                        // Process the tasks in the action
                        foreach (var item in action.Tasks) {

                            // Process the task
                            Console.WriteLine(item.Task.FullPrint());
                            game.Game.Process(item.Task);
                        }
                    }
                }
            }

            Console.WriteLine($"Game: {game.Game.State}, Player1: {game.Player1.PlayState} / Player2: {game.Player2.PlayState}");
        }

    }

}
