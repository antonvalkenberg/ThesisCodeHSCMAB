using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using AVThesis.Enums;
using AVThesis.SabberStone;
using AVThesis.SabberStone.Bots;
using AVThesis.SabberStone.Strategies;
using SabberStoneCore.Config;
using SabberStoneCore.Enums;
using SabberStoneCore.Tasks.PlayerTasks;

/// <summary>
/// Written by A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by G.J. Roelofs and T. Aliyev.
/// </summary>
namespace AVThesis.Tournament {

    /// <summary>
    /// A single tournament match, consisting of an arbitrary amount of games between two bots.
    /// </summary>
    public class TournamentMatch {

        #region Fields

        private readonly bool _printToConsole;

        #endregion

        #region Properties

        /// <summary>
        /// The bots participating in this match.
        /// Note: the order of this collection does not reflect playing order in the match's games. (starting player is alternated)
        /// </summary>
        public List<ISabberStoneBot> Bots { get; set; }

        /// <summary>
        /// The configuration of the SabberStone Game that will be played.
        /// </summary>
        public GameConfig GameConfig { get; set; }

        /// <summary>
        /// The amount of games that should be played in this match.
        /// </summary>
        public int NumberOfGames { get; set; }

        /// <summary>
        /// Statistics regarding the match.
        /// </summary>
        public MatchStatistics MatchStatistics { get; set; }

        /// <summary>
        /// The path to the file where any <see cref="Exception"/>s thrown during processing of <see cref="SabberStoneCore.Tasks.PlayerTask"/>s are written.
        /// </summary>
        public string ExceptionFilePath = Path.Combine(Path.GetDirectoryName(typeof(TournamentMatch).Assembly.Location), "Exceptions.txt");

        #endregion

        #region Constructor

        /// <summary>
        /// Constructs a new instance of a TournamentMatch with a specific GameConfig between two bots.
        /// Note: the order of the bots does not reflect the playing order in the match's games. (starting player is alternated)
        /// </summary>
        /// <param name="bot1Setup">The setup for the first bot.</param>
        /// <param name="bot2Setup">The setup for the second bot.</param>
        /// <param name="configuration">The SabberStone Game configuration for this match.</param>
        /// <param name="numberOfGames">The amount of games that should be played in this match.</param>
        /// <param name="printToConsole">[Optional] Whether or not to print game information to the Console.</param>
        public TournamentMatch(BotSetupType bot1Setup, BotSetupType bot2Setup, GameConfig configuration, int numberOfGames, bool printToConsole = false) {
            Bots = new List<ISabberStoneBot> { BotFactory.CreateSabberStoneBot(bot1Setup), BotFactory.CreateSabberStoneBot(bot2Setup) };
            GameConfig = configuration;
            NumberOfGames = numberOfGames;
            MatchStatistics = new MatchStatistics($"{bot1Setup.ToString()}[{configuration.Player1Name}]", $"{bot2Setup.ToString()}[{configuration.Player2Name}]", numberOfGames);
            _printToConsole = printToConsole;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns a SabberStone game configuration setup for a tournament match.
        /// </summary>
        /// <returns>A SabberStone game configuration setup for a tournament match.</returns>
        public static GameConfig GetTournamentConfiguration() {
            return new GameConfig {
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
            };
        }

        /// <summary>
        /// Runs the entire match. Currently sequentially runs each game.
        /// </summary>
        public void RunMatch() {
            // Run all the games of the match.
            for (var i = 0; i < NumberOfGames; i++) {
                RunGame(i);
            }

            // Run all the games of the match.
            // Note: this causes issues because of the event used during playout (PlayoutStrategySabberStone.SimulationCompleted).
            //Parallel.For(0, NumberOfGames, RunGame);
        }

        /// <summary>
        /// Runs a game of this match with the specified index.
        /// </summary>
        /// <param name="gameIndex">The index of the game that should be run.</param>
        public void RunGame(int gameIndex) {
            Console.WriteLine($"** Starting Game {gameIndex+1} of {NumberOfGames}");
            try {
                var timer = Stopwatch.StartNew();

                // Alternate which player starts.
                var config = GameConfig.Clone();
                config.StartPlayer = gameIndex % 2 + 1;

                // Create a new game with the cloned configuration.
                var game = new SabberStoneState(new SabberStoneCore.Model.Game(config));

                // Set up the bots with their Controller from the created game.
                Bots[0].SetController(game.Player1);
                Bots[1].SetController(game.Player2);

                // Get the game ready.
                game.Game.StartGame();
                MatchStatistics.NewGameStarted(gameIndex + 1, game.Game.FirstPlayer.Name);

                // Default mulligan for each player.
                game.Game.Process(MulliganStrategySabberStone.DefaultMulligan(game.Game.Player1));
                game.Game.Process(MulliganStrategySabberStone.DefaultMulligan(game.Game.Player2));

                game.Game.MainReady();

                // Play out the game.
                while (game.Game.State != State.COMPLETE) {
                    if (_printToConsole) Console.WriteLine("");
                    if (_printToConsole) Console.WriteLine($"*TURN {(game.Game.Turn + 1) / 2} - {game.Game.CurrentPlayer.Name}");
                    if (_printToConsole) Console.WriteLine($"*Hero[P1] {game.Player1.Hero} HP: {game.Player1.Hero.Health} / Hero[P2] {game.Player2.Hero} HP: {game.Player2.Hero.Health}");

                    // Play out the current player's turn until they pass.
                    if (game.Game.CurrentPlayer.Id == Bots[0].PlayerID()) PlayPlayerTurn(game, Bots[0]);
                    else if (game.Game.CurrentPlayer.Id == Bots[1].PlayerID()) PlayPlayerTurn(game, Bots[1]);
                }

                if (_printToConsole) {
                    Console.WriteLine($"*Game: {game.Game.State}, Player1: {game.Player1.PlayState} / Player2: {game.Player2.PlayState}");
                    Console.WriteLine($"*Game lasted {timer.Elapsed:g}");
                }

                // Create game data.
                MatchStatistics.EndCurrentGame(game);
            }
            catch (Exception e) {
                Console.WriteLine($"ERROR: Exception thrown during game {gameIndex+1}");
                WriteExceptionToFile(e);
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Plays out a player's turn.
        /// Note: this method continuously asks the bot to Act and stops when 'null' is returned.
        /// </summary>
        /// <param name="game">The current game state.</param>
        /// <param name="bot">The bot that should play the turn.</param>
        private void PlayPlayerTurn(SabberStoneState game, ISabberStoneBot bot) {
            var currentPlayerName = game.Game.CurrentPlayer.Name;
            if (_printToConsole) Console.WriteLine($"- <{currentPlayerName}> ---------------------------");
            var timer = Stopwatch.StartNew();

            // Ask the bot to act.
            var action = bot.Act(game);
            timer.Stop();

            // In the case where an incomplete action was returned, add end-turn
            if (!action.IsComplete()) {
                Console.WriteLine("WARNING: Incomplete action received. Adding EndTurn.");
                action.AddTask((SabberStonePlayerTask)EndTurnTask.Any(game.Game.CurrentPlayer));
            }

            // Process the tasks in the action
            var executedTasks = new List<SabberStonePlayerTask>();
            foreach (var item in action.Tasks) {

                if (_printToConsole) Console.WriteLine(item.Task.FullPrint());
                try {
                    // Process the task
                    game.Game.Process(item.Task);
                }
                catch (Exception e) {
                    Console.WriteLine($"ERROR: Exception thrown while processing task {item.Task}");
                    WriteExceptionToFile(e, item);
                    // If the game is still running and the current player is still active, pass the turn
                    if (game.Game.CurrentPlayer.Id == bot.PlayerID())
                        game.Game.Process(EndTurnTask.Any(game.Game.CurrentPlayer));
                    // Do not continue with any other tasks in this action
                    break;
                }
                finally {
                    executedTasks.Add(item);
                }
            }

            // Store the action in the match-statistics
            MatchStatistics.ProcessAction(currentPlayerName, executedTasks, timer.Elapsed);
            if (_printToConsole) Console.WriteLine($"*Action computation time: {timer.Elapsed:g}");
        }

        /// <summary>
        /// Writes the provided <see cref="Exception"/> to the file defined in <see cref="ExceptionFilePath"/>.
        /// </summary>
        /// <param name="exception">The exception to write to the file.</param>
        private void WriteExceptionToFile(Exception exception) {
            var writer = new StreamWriter(ExceptionFilePath, true);
            writer.WriteLine("");
            writer.WriteLine(exception.Message);
            writer.WriteLine(exception.StackTrace);
            writer.WriteLine("");
            writer.Close();
        }

        /// <summary>
        /// Writes the provided <see cref="Exception"/> to the file defined in <see cref="ExceptionFilePath"/>.
        /// Adds some additional information about which task caused the exception.
        /// </summary>
        /// <param name="exception">The exception to write to the file.</param>
        /// <param name="task">The <see cref="SabberStonePlayerTask"/> that caused the exception.</param>
        private void WriteExceptionToFile(Exception exception, SabberStonePlayerTask task) {
            var writer = new StreamWriter(ExceptionFilePath, true);
            try {
                writer.WriteLine(task);
                writer.WriteLine("Caused:");
                writer.WriteLine(exception.Message);
                writer.WriteLine(exception.StackTrace);
                writer.WriteLine("");
            }
            catch (Exception e) {
                Console.WriteLine(e);
            }
            finally {
                writer.Close();
            }
        }

        #endregion

    }
}
