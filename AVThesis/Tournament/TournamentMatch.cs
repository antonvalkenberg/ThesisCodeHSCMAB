using System;
using System.Collections.Generic;
using AVThesis.SabberStone;
using SabberStoneCore.Config;
using SabberStoneCore.Enums;
using System.Linq;

/// <summary>
/// Written by A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by G.J. Roelofs and T. Aliyev.
/// </summary>
namespace AVThesis.Tournament {

    /// <summary>
    /// A single tournament match, consisting of an arbitrary amount of games between two bots.
    /// </summary>
    public class TournamentMatch {

        #region Inner Class: GameData

        public class GameData {
            private int _winningPlayerID;

            public int WinningPlayerID { get => _winningPlayerID; set => _winningPlayerID = value; }
            
            public GameData(int winningPlayerID) {
                WinningPlayerID = winningPlayerID;
            }

        }

        #endregion

        #region Fields

        List<ISabberStoneBot> _bots;
        GameConfig _gameConfig;
        int _numberOfGames;
        List<GameData> _matchStatistics;
        bool _printToConsole = false;

        #endregion

        #region Properties

        /// <summary>
        /// The bots participating in this match.
        /// Note: the order of this collection does not reflect playing order in the match's games. (starting player is alternated)
        /// </summary>
        public List<ISabberStoneBot> Bots { get => _bots; set => _bots = value; }

        /// <summary>
        /// The configuration of the SabberStone Game that will be played.
        /// </summary>
        public GameConfig GameConfig { get => _gameConfig; set => _gameConfig = value; }

        /// <summary>
        /// The amount of games that should be played in this match.
        /// </summary>
        public int NumberOfGames { get => _numberOfGames; set => _numberOfGames = value; }

        /// <summary>
        /// Statistics regarding the match, indexed by game number.
        /// </summary>
        public List<GameData> MatchStatistics { get => _matchStatistics; set => _matchStatistics = value; }
        
        #endregion

        #region Constructor

        /// <summary>
        /// Constructs a new instance of a TournamentMatch with a specific GameConfig between two bots.
        /// Note: the order of the bots does not reflect the playing order in the match's games. (starting player is alternated)
        /// </summary>
        /// <param name="bot1">The first bot.</param>
        /// <param name="bot2">The second bot.</param>
        /// <param name="configuration">The SabberStone Game configuration for this match.</param>
        /// <param name="numberOfGames">The amount of games that should be played in this match.</param>
        /// <param name="printToConsole">[Optional] Whether or not to print game information to the Console.</param>
        public TournamentMatch(ISabberStoneBot bot1, ISabberStoneBot bot2, GameConfig configuration, int numberOfGames, bool printToConsole = false) {
            Bots = new List<ISabberStoneBot> { bot1, bot2 };
            GameConfig = configuration;
            NumberOfGames = numberOfGames;
            MatchStatistics = new List<GameData>(numberOfGames);
            _printToConsole = printToConsole;
        }

        #endregion

        #region Public Methods
        
        /// <summary>
        /// Runs the entire match. Currently sequentially runs each game.
        /// </summary>
        public void RunMatch() {
            // Run all the games of the match.
            for (int i = 0; i < NumberOfGames; i++) {
                Console.WriteLine(string.Format("** Starting Game {0} of {1}", i, NumberOfGames));
                RunGame(i);
                Console.WriteLine("");
            }

            // Check how many games each bot won at the end.
            int bot1Wins = MatchStatistics.Where(i => i.WinningPlayerID == Bots[0].PlayerID()).Count();
            int bot2Wins = MatchStatistics.Where(i => i.WinningPlayerID == Bots[1].PlayerID()).Count();
            Console.WriteLine(string.Format("** Match Results: {0} won {1} games, {2} won {3} games.", Bots[0].Name(), bot1Wins, Bots[1].Name(), bot2Wins));
        }

        /// <summary>
        /// Runs a game of this match with the specified index.
        /// </summary>
        /// <param name="gameIndex">The index of the game that should be run.</param>
        public void RunGame(int gameIndex) {
            
            // Alternate which player starts.
            var config = GameConfig.Clone();
            config.StartPlayer = (gameIndex % 2) + 1;
            
            // Create a new game with the cloned configuration.
            var game = new SabberStoneState(new SabberStoneCore.Model.Game(config));

            // Set up the bots with their Controller from the created game.
            Bots[0].SetController(game.Player1);
            Bots[1].SetController(game.Player2);

            // Get the game ready.
            game.Game.StartGame();
            
            // Play out the game.
            while (game.Game.State != State.COMPLETE) {
                if (_printToConsole) Console.WriteLine("");
                if (_printToConsole) Console.WriteLine($"Player1: {game.Player1.PlayState} / Player2: {game.Player2.PlayState} - " + $"TURN {(game.Game.Turn + 1) / 2} - {game.Game.CurrentPlayer.Name}");
                if (_printToConsole) Console.WriteLine($"Hero[P1] {game.Player1.Hero} HP: {game.Player1.Hero.Health} / Hero[P2] {game.Player2.Hero} HP: {game.Player2.Hero.Health}");

                // Play out the current player's turn until they pass.
                if (game.Game.CurrentPlayer.Id == Bots[0].PlayerID()) PlayPlayerTurn(game, Bots[0]);
                else if (game.Game.CurrentPlayer.Id == Bots[1].PlayerID()) PlayPlayerTurn(game, Bots[1]);
            }

            if (_printToConsole) Console.WriteLine($"Game: {game.Game.State}, Player1: {game.Player1.PlayState} / Player2: {game.Player2.PlayState}");

            // Create game data.
            MatchStatistics.Insert(gameIndex, new GameData(game.PlayerWon));
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Plays out a player's turn.
        /// Note: this method continously asks the bot to Act and stops when 'null' is returned.
        /// </summary>
        /// <param name="game">The current game state.</param>
        /// <param name="bot">The bot that should play the turn.</param>
        private void PlayPlayerTurn(SabberStoneState game, ISabberStoneBot bot) {
            // Handle all of the player's turn
            if (_printToConsole) Console.WriteLine($"- <{game.Game.CurrentPlayer.Name}> ---------------------------");

            // Check if the bot is for the current player
            if (game.Game.CurrentPlayer.Id == bot.PlayerID()) {

                // Ask the bot to act.
                var action = bot.Act(game);
                
                // Check if the action is valid
                if (action.IsValid()) {

                    // Process the tasks in the action
                    foreach (var item in action.Tasks) {

                        // Process the task
                        if (_printToConsole) Console.WriteLine(item.FullPrint());
                        game.Game.Process(item);
                    }
                }
            }
        }

        #endregion

    }
}
