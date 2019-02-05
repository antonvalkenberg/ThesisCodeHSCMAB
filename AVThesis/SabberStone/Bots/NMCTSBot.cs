using System;
using System.Collections.Generic;
using System.Linq;
using AVThesis.Game;
using AVThesis.SabberStone.Strategies;
using AVThesis.Search;
using AVThesis.Search.Tree.NMC;
using SabberStoneCore.Model.Entities;
using SabberStoneCore.Tasks.PlayerTasks;

/// <summary>
/// Written by A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by G.J. Roelofs and T. Aliyev.
/// </summary>
namespace AVThesis.SabberStone.Bots {

    /// <summary>
    /// A bot that plays Hearthstone using Monte Carlo Tree Search with Naïve Sampling to find its best move.
    /// </summary>
    public class NMCTSBot : ISabberStoneBot {

        #region Inner Classes

        /// <summary>
        /// Handles the local exploration for NMCTS.
        /// </summary>
        private class SabberStoneNMCTSExplorationStrategy : IExplorationStrategy<List<SabberStoneAction>, SabberStoneState, SabberStoneAction, object, SabberStoneAction> {

            #region Properties

            /// <summary>
            /// The policy that defines the local exploration.
            /// </summary>
            private double LocalPolicy { get; }

            #endregion

            #region Constructors

            /// <summary>
            /// Creates a new instance of this exploration strategy.
            /// </summary>
            /// <param name="localPolicy">The local policy to use.</param>
            public SabberStoneNMCTSExplorationStrategy(double localPolicy) {
                LocalPolicy = localPolicy;
            }

            #endregion

            #region Public Methods

            /// <inheritdoc />
            public bool Policy(SearchContext<List<SabberStoneAction>, SabberStoneState, SabberStoneAction, object, SabberStoneAction> context, int currentIteration) {
                return Util.RNG.NextDouble() > LocalPolicy;
            }

            #endregion

        }

        /// <summary>
        /// Handles the sampling of states during the NaïveSampling process.
        /// </summary>
        private class SabberStoneNMCSamplingStrategy : ISamplingStrategy<SabberStoneState, SabberStoneAction> {

            #region Properties

            /// <summary>
            /// The bot that creates the sample actions.
            /// </summary>
            private RandomBot NaïveBot { get; }

            #endregion

            #region Constructors

            /// <summary>
            /// Constructs a new instance.
            /// </summary>
            /// <param name="naïveBot">The bot to use when creating samples.</param>
            public SabberStoneNMCSamplingStrategy(RandomBot naïveBot) {
                NaïveBot = naïveBot;
            }

            #endregion

            #region Public Methods

            /// <inheritdoc />
            public SabberStoneAction Sample(SabberStoneState state) {
                return NaïveBot.CreateRandomAction(state);
            }

            #endregion

        }

        #endregion

        #region Constants

        private const int NMCTS_NUMBER_OF_ITERATIONS = 10000;
        private const int PLAYOUT_TURN_CUTOFF = 3;
        private const double NMCTS_GLOBAL_POLICY = 0.2;
        private const double NMCTS_LOCAL_POLICY = 0.2;
        private const double UCT_C_CONSTANT_DEFAULT = 0.1;
        private const string BOT_NAME = "NMCTSBot";
        private readonly bool _debug;

        #endregion

        #region Properties

        /// <summary>
        /// The player this bot is representing in a game of SabberStone.
        /// </summary>
        public Controller Player { get; set; }

        /// <summary>
        /// The bot that is used during the playouts.
        /// </summary>
        public ISabberStoneBot MyPlayoutBot { get; set; }

        /// <summary>
        /// The bot that is used for the opponent's playouts.
        /// </summary>
        public ISabberStoneBot OpponentPlayoutBot { get; set; }

        /// <summary>
        /// A bot used during the NaïveSampling process.
        /// </summary>
        public RandomBot RandomSamplingBot { get; set; }

        /// <summary>
        /// The strategy used to determine if a playout has reached its goal state.
        /// </summary>
        public IGoalStrategy<List<SabberStoneAction>, SabberStoneState, SabberStoneAction, object, SabberStoneAction> Goal { get; set; }

        /// <summary>
        /// The game specific logic required for searching through SabberStoneStates and SabberStoneActions
        /// </summary>
        public IGameLogic<List<SabberStoneAction>, SabberStoneState, SabberStoneAction, object, SabberStoneAction, SabberStoneAction> GameLogic { get; set; }

        /// <summary>
        /// The strategy used to play out a game in simulation.
        /// </summary>
        public IPlayoutStrategy<List<SabberStoneAction>, SabberStoneState, SabberStoneAction, object, SabberStoneAction> Playout { get; set; }

        /// <summary>
        /// The Naïve Monte Carlo Tree Search builder that creates the search-setup ready to use.
        /// </summary>
        public NMCTSBuilder<List<SabberStoneAction>, SabberStoneState, SabberStoneAction, object, SabberStoneAction> Builder { get; set; }

        /// <summary>
        /// Whether or not this bot is allowed perfect information about the game state (i.e. no obfuscation and therefore no determinisation).
        /// </summary>
        public bool PerfectInformation { get; set; }

        /// <summary>
        /// The size of the ensemble the search should use.
        /// </summary>
        public int EnsembleSize { get; set; }

        /// <summary>
        /// The ensemble strategy to use.
        /// </summary>
        public EnsembleStrategySabberStone Ensemble { get; set; }

        /// <summary>
        /// The solutions received from the ensemble.
        /// </summary>
        public List<SabberStoneAction> EnsembleSolutions { get; set; }

        /// <summary>
        /// Does the administrative tasks around searching.
        /// </summary>
        public SabberStoneSearch Searcher { get; set; }

        /// <summary>
        /// The type of selection strategy used by the MAST playout.
        /// </summary>
        public MASTPlayoutBot.SelectionType MASTSelectionType { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Constructs a new instance of NMCTSBot with a <see cref="Controller"/> representing the player.
        /// </summary>
        /// <param name="player">The player.</param>
        /// <param name="allowPerfectInformation">[Optional] Whether or not this bot is allowed perfect information about the game state (i.e. no obfuscation and therefore no determinisation). Default value is false.</param>
        /// <param name="ensembleSize">[Optional] The size of the ensemble to use. Default value is 1.</param>
        /// <param name="mastSelectionType">[Optional] The type of selection strategy used by the MAST playout. Default value is <see cref="MASTPlayoutBot.SelectionType.EGreedy"/>.</param>
        /// <param name="debugInfoToConsole">[Optional] Whether or not to write debug information to the console. Default value is false.</param>
        public NMCTSBot(Controller player, bool allowPerfectInformation = false, int ensembleSize = 1, MASTPlayoutBot.SelectionType mastSelectionType = MASTPlayoutBot.SelectionType.EGreedy, bool debugInfoToConsole = false)
            : this(allowPerfectInformation, ensembleSize, mastSelectionType, debugInfoToConsole) {
            Player = player;

            // Set the playout bots correctly if we are using PlayoutStrategySabberStone
            if (Playout is PlayoutStrategySabberStone playout) {
                MyPlayoutBot.SetController(Player);
                playout.AddPlayoutBot(Player.Id, MyPlayoutBot);
                OpponentPlayoutBot.SetController(Player.Opponent);
                playout.AddPlayoutBot(Player.Id, MyPlayoutBot);
            }

            // Set the controller of the random sampling bot
            RandomSamplingBot.SetController(Player);

            // Create the searcher that will handle the searching and some administrative tasks
            Searcher = new SabberStoneSearch(Player, _debug);
        }

        /// <summary>
        /// Constructs a new instance of NMCTSBot with default strategies.
        /// </summary>
        /// <param name="allowPerfectInformation">[Optional] Whether or not this bot is allowed perfect information about the game state (i.e. no obfuscation and therefore no determinisation). Default value is false.</param>
        /// <param name="ensembleSize">[Optional] The size of the ensemble to use. Default value is 1.</param>
        /// <param name="mastSelectionType">[Optional] The type of selection strategy used by the MAST playout. Default value is <see cref="MASTPlayoutBot.SelectionType.EGreedy"/>.</param>
        /// <param name="debugInfoToConsole">[Optional] Whether or not to write debug information to the console. Default value is false.</param>
        public NMCTSBot(bool allowPerfectInformation = false, int ensembleSize = 1, MASTPlayoutBot.SelectionType mastSelectionType = MASTPlayoutBot.SelectionType.EGreedy, bool debugInfoToConsole = false) {
            PerfectInformation = allowPerfectInformation;
            EnsembleSize = ensembleSize;
            MASTSelectionType = mastSelectionType;
            _debug = debugInfoToConsole;

            // Create the ensemble search
            Ensemble = new EnsembleStrategySabberStone(enableStateObfuscation: true, enablePerfectInformation: PerfectInformation);

            // Simulation will be handled by the Playout
            var sabberStoneStateEvaluation = new EvaluationStrategyHearthStone();
            var playout = new PlayoutStrategySabberStone();
            Playout = playout;

            // Set the playout bots
            MyPlayoutBot = new MASTPlayoutBot(MASTSelectionType, sabberStoneStateEvaluation, playout);
            OpponentPlayoutBot = new MASTPlayoutBot(MASTSelectionType, sabberStoneStateEvaluation, playout);
            // And the random sampling bot
            RandomSamplingBot = new RandomBot();

            // We'll be cutting off the simulations after X turns, using a GoalStrategy
            Goal = new GoalStrategyTurnCutoff(PLAYOUT_TURN_CUTOFF);

            // Application and Goal will be handled by the GameLogic
            GameLogic = new SabberStoneGameLogic(false, Goal);

            // Build NMCTS
            Builder = NMCTS<List<SabberStoneAction>, SabberStoneState, SabberStoneAction, object, SabberStoneAction>.Builder();
            Builder.ExplorationStrategy = new SabberStoneNMCTSExplorationStrategy(NMCTS_LOCAL_POLICY);
            Builder.PlayoutStrategy = Playout;
            Builder.PolicyGlobal = NMCTS_GLOBAL_POLICY;
            Builder.SamplingStrategy = new SabberStoneNMCSamplingStrategy(RandomSamplingBot);
            Builder.SolutionStrategy = new SolutionStrategySabberStone(false, new AverageScore<SabberStoneState, SabberStoneAction>());
            Builder.EvaluationStrategy = sabberStoneStateEvaluation;
            Builder.Iterations = EnsembleSize > 0 ? NMCTS_NUMBER_OF_ITERATIONS / EnsembleSize : NMCTS_NUMBER_OF_ITERATIONS; // Note: Integer division by design.
        }

        #endregion

        #region Public Methods

        #region ISabberStoneBot

        /// <inheritdoc />
        public SabberStoneAction Act(SabberStoneState state) {
            var timer = System.Diagnostics.Stopwatch.StartNew();
            var gameState = (SabberStoneState)state.Copy();

            if (_debug) Console.WriteLine();
            if (_debug) Console.WriteLine(Name());
            if (_debug) Console.WriteLine($"Starting an ({EnsembleSize})Ensemble-NMCTS-search in turn {(gameState.Game.Turn + 1) / 2}");

            // Setup and start the ensemble-search
            EnsembleSolutions = new List<SabberStoneAction>();
            var search = (NMCTS<List<SabberStoneAction>, SabberStoneState, SabberStoneAction, object, SabberStoneAction>)Builder.Build();
            var context = SearchContext<List<SabberStoneAction>, SabberStoneState, SabberStoneAction, object, SabberStoneAction>.GameSearchSetup(GameLogic, EnsembleSolutions, gameState, null, search);
            Ensemble.EnsembleSearch(context, Searcher.Search, EnsembleSize);

            // Determine the best tasks to play based on the ensemble search, or just take the one in case of a single search.
            var solution = EnsembleSize > 1 ? Searcher.DetermineBestTasks(state) : EnsembleSolutions.First();

            var time = timer.ElapsedMilliseconds;
            if (_debug) Console.WriteLine();
            if (_debug) Console.WriteLine($"Ensemble-NMCTS returned with solution: {solution}");
            if (_debug) Console.WriteLine($"My total calculation time was: {time} ms.");

            // Check if the solution is a complete action.
            if (!solution.IsComplete()) {
                // Otherwise add an End-Turn task before returning.
                if (_debug) Console.WriteLine("Solution was an incomplete action; adding End-Turn task.");
                solution.Tasks.Add((SabberStonePlayerTask)EndTurnTask.Any(Player));
            }

            if (_debug) Console.WriteLine();
            return solution;
        }

        /// <inheritdoc />
        public void SetController(Controller controller) {
            Player = controller;
        }

        /// <inheritdoc />
        public int PlayerID() {
            return Player.Id;
        }

        /// <inheritdoc />
        public string Name() {
            var pi = PerfectInformation ? "_PI" : "";
            return $"{BOT_NAME}_{Builder.Iterations}it_es{EnsembleSize}{pi}_p{MyPlayoutBot.Name()}_op{OpponentPlayoutBot.Name()}";
        }
        
        #endregion

        #endregion

    }

}
