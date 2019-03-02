using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AVThesis.SabberStone;
using SabberStoneCore.Enums;

/// <summary>
/// Written by A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by G.J. Roelofs and T. Aliyev.
/// </summary>
namespace AVThesis.Tournament {

    /// <summary>
    /// Contains information about a <see cref="TournamentMatch"/>.
    /// </summary>
    public class MatchStatistics {

        #region Inner Classes

        /// <summary>
        /// Contains information about a single game in a <see cref="TournamentMatch"/>.
        /// </summary>
        public class GameStatistics {

            #region Properties

            /// <summary>
            /// The number of this game within the match.
            /// </summary>
            public int GameNumber { get; set; }

            /// <summary>
            /// Name of the player that started this game.
            /// </summary>
            public string StartingPlayerName { get; set; }

            /// <summary>
            /// The status of this game.
            /// </summary>
            public State Status { get; set; }

            /// <summary>
            /// The name of Player1 within this game's <see cref="SabberStoneCore.Model.Game"/>.
            /// </summary>
            public string Player1Name { get; set; }

            /// <summary>
            /// The name of Player2 within this game's <see cref="SabberStoneCore.Model.Game"/>.
            /// </summary>
            public string Player2Name { get; set; }

            /// <summary>
            /// The health points of Player1 at the end of the game.
            /// </summary>
            public int Player1HP { get; set; }

            /// <summary>
            /// The health points of Player2 at the end of the game.
            /// </summary>
            public int Player2HP { get; set; }

            /// <summary>
            /// The turn count when the game was completed.
            /// </summary>
            public int FinalTurn { get; set; }

            /// <summary>
            /// The actions executed in this game along with the computing time used, indexed by player name.
            /// </summary>
            public Dictionary<string, List<Tuple<List<SabberStonePlayerTask>, TimeSpan>>> GameActions { get; set; }

            #endregion

            #region Constructors

            /// <summary>
            /// Creates a new instance.
            /// </summary>
            /// <param name="gameNumber">The number of this game within the match.</param>
            /// <param name="player1Name">The name of Player1 within this game's <see cref="SabberStoneCore.Model.Game"/>.</param>
            /// <param name="player2Name">The name of Player2 within this game's <see cref="SabberStoneCore.Model.Game"/>.</param>
            /// <param name="startingPlayerName">Name of the player that started this game.</param>
            public GameStatistics(int gameNumber, string player1Name, string player2Name, string startingPlayerName) {
                GameNumber = gameNumber;
                Player1Name = player1Name;
                Player2Name = player2Name;
                StartingPlayerName = startingPlayerName;
                Status = State.RUNNING;
                GameActions = new Dictionary<string, List<Tuple<List<SabberStonePlayerTask>, TimeSpan>>>() {{Player1Name, new List<Tuple<List<SabberStonePlayerTask>, TimeSpan>>()}, {Player2Name, new List<Tuple<List<SabberStonePlayerTask>, TimeSpan>>()}};
            }

            #endregion

            #region Public Methods

            /// <summary>
            /// Finalise these statistics using the completed game's state and write the information to a file.
            /// </summary>
            /// <param name="state">The state of the completed game.</param>
            public void Finalise(SabberStoneState state) {
                Player1HP = state.Player1.Hero.Health;
                Player2HP = state.Player2.Hero.Health;
                FinalTurn = state.Game.Turn;
                Status = state.Game.State;
                WriteToFile();
            }

            /// <summary>
            /// Add an executed action to these statistics.
            /// </summary>
            /// <param name="player">The name of the player that performed the action.</param>
            /// <param name="action">The tasks that were executed.</param>
            /// <param name="actionTime">The time that was spent on computing the action.</param>
            public void AddAction(string player, List<SabberStonePlayerTask> tasks, TimeSpan actionTime) {
                GameActions[player].Add(new Tuple<List<SabberStonePlayerTask>, TimeSpan>(tasks, actionTime));
            }

            /// <summary>
            /// Determines the name of the player that won the game, if any.
            /// </summary>
            /// <returns>The name of the player that won the game, `DRAW' if the game was a draw and `INCOMPLETE' if the game's status is not <see cref="State.COMPLETE"/>.</returns>
            public string WinningPlayer() {
                if (Status != State.COMPLETE)
                    return "INCOMPLETE";
                if (Player1HP <= 0)
                    return Player2HP <= 0 ? "DRAW" : Player2Name;
                return Player1Name;
            }

            /// <summary>
            /// Writes these statistics to a separate file.
            /// </summary>
            public void WriteToFile() {
                var fileName = $"{Player1Name}-{Player2Name}-{GameNumber}.txt";
                var writer = new StreamWriter(Path.Combine(Path.GetDirectoryName(typeof(GameStatistics).Assembly.Location), fileName));
                writer.WriteLine($"Winning player: {WinningPlayer()}");
                writer.WriteLine($"Starting player: {StartingPlayerName}");
                writer.WriteLine($"Player1HP: {Player1HP}");
                writer.WriteLine($"Player2HP: {Player2HP}");
                writer.WriteLine($"EndTurn: {FinalTurn} or {(FinalTurn + 1) / 2}");
                writer.WriteLine("");
                writer.WriteLine("Game Actions:");
                writer.WriteLine("");
                foreach (var kvPair in GameActions) {
                    writer.WriteLine($"*{kvPair.Key} - Total Computing Time: {TimeSpan.FromMilliseconds(kvPair.Value.Sum(i => i.Item2.TotalMilliseconds)):g}");
                    foreach (var tuple in kvPair.Value) {
                        writer.WriteLine($"Time: {tuple.Item2:g}");
                        writer.WriteLine(tuple.Item1);
                    }
                }
                writer.Close();
            }

            #endregion

        }

        #endregion

        #region Properties

        /// <summary>
        /// The games contained in this match.
        /// </summary>
        public List<GameStatistics> Games { get; set; }

        /// <summary>
        /// The name of Player1 within this match's <see cref="SabberStoneCore.Model.Game"/>s.
        /// </summary>
        public string Player1 { get; set; }

        /// <summary>
        /// The name of Player2 within this match's <see cref="SabberStoneCore.Model.Game"/>s.
        /// </summary>
        public string Player2 { get; set; }

        /// <summary>
        /// The game that is currently in progress in this match.
        /// </summary>
        public GameStatistics CurrentGame { get; set; }

        /// <summary>
        /// The path to the file that contains the game-results for this match.
        /// </summary>
        public string ResultsFilePath { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="player1">The name of Player1 within this match's <see cref="SabberStoneCore.Model.Game"/>s.</param>
        /// <param name="player2">The name of Player2 within this match's <see cref="SabberStoneCore.Model.Game"/>s.</param>
        /// <param name="numberOfGames">The number of games that this match lasts for.</param>
        public MatchStatistics(string player1, string player2, int numberOfGames) {
            Player1 = player1;
            Player2 = player2;
            Games = new List<GameStatistics>(numberOfGames);

            ResultsFilePath = Path.Combine(Path.GetDirectoryName(typeof(MatchStatistics).Assembly.Location), $"{Player1}-{Player2}.txt");
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Writes a single game's results to the results file.
        /// </summary>
        private void WriteGameResultToFile() {
            var writer = new StreamWriter(ResultsFilePath, true);
            writer.WriteLine($"{CurrentGame.WinningPlayer()},{CurrentGame.Player1HP},{CurrentGame.Player2HP},{CurrentGame.FinalTurn}");
            writer.Close();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Process that a new game has started within this match.
        /// </summary>
        /// <param name="gameNumber">The number of the game within the match.</param>
        /// <param name="player1">The name of Player1 within this match's <see cref="SabberStoneCore.Model.Game"/>s.</param>
        /// <param name="player2">The name of Player2 within this match's <see cref="SabberStoneCore.Model.Game"/>s.</param>
        /// <param name="startingPlayerName">The player that starts this game.</param>
        public void NewGameStarted(int gameNumber, string player1, string player2, string startingPlayerName) {
            CurrentGame = new GameStatistics(gameNumber, player1, player2, startingPlayerName);
        }

        /// <summary>
        /// Process an action performed in the current game.
        /// </summary>
        /// <param name="player">The name of the player that performed the action.</param>
        /// <param name="tasks">The tasks that were executed as part of the action.</param>
        /// <param name="actionTime">The computation time that was spent on the action.</param>
        public void ProcessAction(string player, List<SabberStonePlayerTask> tasks, TimeSpan actionTime) {
            CurrentGame.AddAction(player, tasks, actionTime);
        }

        /// <summary>
        /// Process the end of the current game.
        /// </summary>
        /// <param name="state">The end state of the game.</param>
        public void EndCurrentGame(SabberStoneState state) {
            CurrentGame.Finalise(state);
            WriteGameResultToFile();
            Games.Add(CurrentGame);
        }

        #endregion

    }

}
