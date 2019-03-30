using System;
using AVThesis.Enums;
using AVThesis.SabberStone;
using SabberStoneCore.Config;
using SabberStoneCore.Enums;
using AVThesis.SabberStone.Bots;
using AVThesis.SabberStone.Strategies;
using AVThesis.Tournament;

/// <summary>
/// Written by A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by G.J. Roelofs and T. Aliyev.
/// </summary>
namespace AVThesis {

    public class Program {

		public static void Main(string[] args) {
            RunTournamentMatch();
            //RunQuickMatch();
        }

        public static void RunTournamentMatch() {
            // Create a new tournament match
            var match = new TournamentMatch(BotSetupType.RandomBot, BotSetupType.RandomBot, 10);
            match.RunMatch();
        }

        public static void RunQuickMatch() {

            var game = new SabberStoneState(new SabberStoneCore.Model.Game(new GameConfig {
                StartPlayer = 1,
                Player1Name = Constants.SABBERSTONE_GAMECONFIG_PLAYER1_NAME,
                Player1HeroClass = CardClass.HUNTER,
                Player1Deck = Decks.GetRandomTournamentDeck(),
                Player2Name = Constants.SABBERSTONE_GAMECONFIG_PLAYER2_NAME,
                Player2HeroClass = CardClass.HUNTER,
                Player2Deck = Decks.GetRandomTournamentDeck(),
                FillDecks = false,
                Shuffle = true,
                SkipMulligan = false,
                History = false
            }));

            // Create two bots to play
            var bot1 = BotFactory.CreateSabberStoneBot(BotSetupType.RandomBot, game.Player1);
            var bot2 = BotFactory.CreateSabberStoneBot(BotSetupType.RandomBot, game.Player2);

            game.Game.StartGame();

            game.Game.Process(MulliganStrategySabberStone.DefaultMulligan(game.Game.Player1));
            game.Game.Process(MulliganStrategySabberStone.DefaultMulligan(game.Game.Player2));

            game.Game.MainReady();

            while (game.Game.State != State.COMPLETE) {
                Console.WriteLine("");
                Console.WriteLine($"TURN {(game.Game.Turn + 1) / 2} - {game.Game.CurrentPlayer.Name}");
                Console.WriteLine($"Hero[P1] {game.Player1.Hero} HP: {game.Player1.Hero.Health} / Hero[P2] {game.Player2.Hero} HP: {game.Player2.Hero.Health}");
                Console.WriteLine($"- {game.Game.CurrentPlayer.Name} Action ----------------------------");

                // Ask the bot to act.
                var action = game.Game.CurrentPlayer.Id == game.Player1.Id ? bot1.Act(game) : bot2.Act(game);

                // Check if the action is valid
                if (action == null || !action.IsComplete()) continue;

                // Process the tasks in the action
                foreach (var item in action.Tasks) {

                    // Process the task
                    Console.WriteLine(item.Task.FullPrint());
                    game.Game.Process(item.Task);
                }
            }

            Console.WriteLine($"Game: {game.Game.State}, Player1: {game.Player1.PlayState} / Player2: {game.Player2.PlayState}");
        }

    }

}
