using System;
using System.Collections.Generic;
using System.Linq;
using AVThesis.Game;
using AVThesis.SabberStone.Strategies;
using AVThesis.Search;
using AVThesis.Search.Tree;
using AVThesis.Search.Tree.MCTS;
using SabberStoneCore.Model.Entities;
using SabberStoneCore.Tasks.PlayerTasks;

/// <summary>
/// Written by A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by G.J. Roelofs and T. Aliyev.
/// </summary>
namespace AVThesis.SabberStone.Bots {

    /// <summary>
    /// A bot that plays Hearthstone using Monte Carlo Tree Search to find its best move.
    /// </summary>
    public class MCTSBot : ISabberStoneBot {

        #region Constants

        private const string BOT_NAME = "MCTSBot";
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
        /// The Monte Carlo Tree Search builder that creates a search-setup ready to use.
        /// </summary>
        public MCTSBuilder<List<SabberStoneAction>, SabberStoneState, SabberStoneAction, object, SabberStoneAction> Builder { get; set; }

        /// <summary>
        /// Whether or not to use Hierarchical Expansion during the search.
        /// </summary>
        public bool HierarchicalExpansion { get; set; }

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
        /// Whether or not to retain the PlayerTask statistics between searches.
        /// </summary>
        public bool RetainTaskStatistics { get; set; }

        /// <summary>
        /// The type of selection strategy used by the MAST playout.
        /// </summary>
        public MASTPlayoutBot.SelectionType MASTSelectionType { get; set; }

        /// <summary>
        /// The budget for the amount of iterations MCTS can use.
        /// </summary>
        public int Iterations { get; set; }

        /// <summary>
        /// The minimum amount of times a node has to be visited before it can be expanded.
        /// </summary>
        public int MinimumVisitThresholdForExpansion { get; set; }

        /// <summary>
        /// The minimum number of visits before using the NodeEvaluation to select the best node.
        /// </summary>
        public int MinimumVisitThresholdForSelection { get; set; }

        /// <summary>
        /// The cutoff for amount of turns simulated during playout.
        /// </summary>
        public int PlayoutTurnCutoff { get; set; }

        /// <summary>
        /// The value for the `C' constant in the UCB1 formula.
        /// </summary>
        public double UCBConstantC { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructs a new instance of MCTSBot with a <see cref="Controller"/> representing the player.
        /// </summary>
        /// <param name="player">The player.</param>
        /// <param name="hierarchicalExpansion">[Optional] Whether or not to use Hierarchical Expansion. Default value is true.</param>
        /// <param name="allowPerfectInformation">[Optional] Whether or not this bot is allowed perfect information about the game state (i.e. no obfuscation and therefore no determinisation). Default value is false.</param>
        /// <param name="ensembleSize">[Optional] The size of the ensemble to use. Default value is 1.</param>
        /// <param name="mastSelectionType">[Optional] The type of selection strategy used by the M.A.S.T. playout. Default value is <see cref="MASTPlayoutBot.SelectionType.EGreedy"/>.</param>
        /// <param name="retainTaskStatistics">[Optional] Whether or not to retain the PlayerTask statistics between searches. Default value is false.</param>
        /// <param name="iterations">[Optional] The budget for the amount of iterations MCTS can use. Default value is <see cref="Constants.DEFAULT_MCTS_ITERATIONS"/>.</param>
        /// <param name="minimumVisitThresholdForExpansion">[Optional] The minimum amount of times a node has to be visited before it can be expanded. Default value is <see cref="Constants.DEFAULT_MCTS_MINIMUM_VISIT_THRESHOLD_FOR_EXPANSION"/>.</param>
        /// <param name="minimumVisitThresholdForSelection">[Optional] The minimum number of visits before using the NodeEvaluation to select the best node. Default value is <see cref="Constants.DEFAULT_MCTS_MINIMUM_VISIT_THRESHOLD_FOR_SELECTION"/>.</param>
        /// <param name="playoutTurnCutoff">[Optional] The amount of turns after which to stop a simulation. Default value is <see cref="Constants.DEFAULT_PLAYOUT_TURN_CUTOFF"/>.</param>
        /// <param name="ucbConstantC">[Optional] Value for the c-constant in the UCB1 formula. Default value is <see cref="Constants.DEFAULT_UCB1_C"/>.</param>
        /// <param name="debugInfoToConsole">[Optional] Whether or not to write debug information to the console. Default value is false.</param>
        public MCTSBot(Controller player, bool hierarchicalExpansion = true, bool allowPerfectInformation = false, int ensembleSize = 1, MASTPlayoutBot.SelectionType mastSelectionType = MASTPlayoutBot.SelectionType.EGreedy, bool retainTaskStatistics = false, int iterations = Constants.DEFAULT_MCTS_ITERATIONS, int minimumVisitThresholdForExpansion = Constants.DEFAULT_MCTS_MINIMUM_VISIT_THRESHOLD_FOR_EXPANSION, int minimumVisitThresholdForSelection = Constants.DEFAULT_MCTS_MINIMUM_VISIT_THRESHOLD_FOR_SELECTION, int playoutTurnCutoff = Constants.DEFAULT_PLAYOUT_TURN_CUTOFF, double ucbConstantC = Constants.DEFAULT_UCB1_C, bool debugInfoToConsole = false)
            : this(hierarchicalExpansion, allowPerfectInformation, ensembleSize, mastSelectionType, retainTaskStatistics, iterations, minimumVisitThresholdForExpansion, minimumVisitThresholdForSelection, playoutTurnCutoff, ucbConstantC, debugInfoToConsole) {
            Player = player;

            // Set the playout bots correctly if we are using PlayoutStrategySabberStone
            if (Playout is PlayoutStrategySabberStone playout) {
                MyPlayoutBot.SetController(Player);
                playout.AddPlayoutBot(Player.Id, MyPlayoutBot);
                OpponentPlayoutBot.SetController(Player.Opponent);
                playout.AddPlayoutBot(Player.Id, MyPlayoutBot);
            }

            // Create the searcher that will handle the searching and some administrative tasks
            Searcher = new SabberStoneSearch(Player, _debug);
        }

        /// <summary>
        /// Constructs a new instance of MCTSBot with default strategies.
        /// </summary>
        /// <param name="hierarchicalExpansion">[Optional] Whether or not to use Hierarchical Expansion. Default value is true.</param>
        /// <param name="allowPerfectInformation">[Optional] Whether or not this bot is allowed perfect information about the game state (i.e. no obfuscation and therefore no determinisation). Default value is false.</param>
        /// <param name="ensembleSize">[Optional] The size of the ensemble to use. Default value is 1.</param>
        /// <param name="mastSelectionType">[Optional] The type of selection strategy used by the MAST playout. Default value is <see cref="MASTPlayoutBot.SelectionType.EGreedy"/>.</param>
        /// <param name="retainTaskStatistics">[Optional] Whether or not to retain the PlayerTask statistics between searches. Default value is false.</param>
        /// <param name="iterations">[Optional] The budget for the amount of iterations MCTS can use. Default value is <see cref="Constants.DEFAULT_MCTS_ITERATIONS"/>.</param>
        /// <param name="minimumVisitThresholdForExpansion">[Optional] The minimum amount of times a node has to be visited before it can be expanded. Default value is <see cref="Constants.DEFAULT_MCTS_MINIMUM_VISIT_THRESHOLD_FOR_EXPANSION"/>.</param>
        /// <param name="minimumVisitThresholdForSelection">[Optional] The minimum number of visits before using the NodeEvaluation to select the best node. Default value is <see cref="Constants.DEFAULT_MCTS_MINIMUM_VISIT_THRESHOLD_FOR_SELECTION"/>.</param>
        /// <param name="playoutTurnCutoff">[Optional] The amount of turns after which to stop a simulation. Default value is <see cref="Constants.DEFAULT_PLAYOUT_TURN_CUTOFF"/>.</param>
        /// <param name="ucbConstantC">[Optional] Value for the c-constant in the UCB1 formula. Default value is <see cref="Constants.DEFAULT_UCB1_C"/>.</param>
        /// <param name="debugInfoToConsole">[Optional] Whether or not to write debug information to the console. Default value is false.</param>
        public MCTSBot(bool hierarchicalExpansion = true, bool allowPerfectInformation = false, int ensembleSize = 1, MASTPlayoutBot.SelectionType mastSelectionType = MASTPlayoutBot.SelectionType.EGreedy, bool retainTaskStatistics = false, int iterations = Constants.DEFAULT_MCTS_ITERATIONS, int minimumVisitThresholdForExpansion = Constants.DEFAULT_MCTS_MINIMUM_VISIT_THRESHOLD_FOR_EXPANSION, int minimumVisitThresholdForSelection = Constants.DEFAULT_MCTS_MINIMUM_VISIT_THRESHOLD_FOR_SELECTION, int playoutTurnCutoff = Constants.DEFAULT_PLAYOUT_TURN_CUTOFF, double ucbConstantC = Constants.DEFAULT_UCB1_C, bool debugInfoToConsole = false) {
            HierarchicalExpansion = hierarchicalExpansion;
            PerfectInformation = allowPerfectInformation;
            EnsembleSize = ensembleSize;
            MASTSelectionType = mastSelectionType;
            RetainTaskStatistics = retainTaskStatistics;
            Iterations = iterations;
            MinimumVisitThresholdForExpansion = minimumVisitThresholdForExpansion;
            MinimumVisitThresholdForSelection = minimumVisitThresholdForSelection;
            PlayoutTurnCutoff = playoutTurnCutoff;
            UCBConstantC = ucbConstantC;
            _debug = debugInfoToConsole;

            // Create the ensemble search
            Ensemble = new EnsembleStrategySabberStone(enableStateObfuscation: true, enablePerfectInformation: PerfectInformation);

            // Simulation will be handled by the Playout.
            var sabberStoneStateEvaluation = new EvaluationStrategyHearthStone();
            var playout = new PlayoutStrategySabberStone();
            Playout = playout;

            // Set the playout bots
            MyPlayoutBot = new MASTPlayoutBot(MASTSelectionType, sabberStoneStateEvaluation, playout);
            OpponentPlayoutBot = new MASTPlayoutBot(MASTSelectionType, sabberStoneStateEvaluation, playout);

            // We'll be cutting off the simulations after X turns, using a GoalStrategy.
            Goal = new GoalStrategyTurnCutoff(PlayoutTurnCutoff);

            // Expansion, Application and Goal will be handled by the GameLogic.
            GameLogic = new SabberStoneGameLogic(HierarchicalExpansion, Goal);

            // Create the INodeEvaluation strategy used in the selection phase.
            var nodeEvaluation = new ScoreUCB<SabberStoneState, SabberStoneAction>(UCBConstantC);

            // Build MCTS
            Builder = MCTS<List<SabberStoneAction>, SabberStoneState, SabberStoneAction, object, SabberStoneAction>.Builder();
            Builder.ExpansionStrategy = new MinimumTExpansion<List<SabberStoneAction>, SabberStoneState, SabberStoneAction, object, SabberStoneAction>(MinimumVisitThresholdForExpansion);
            Builder.SelectionStrategy = new BestNodeSelection<List<SabberStoneAction>, SabberStoneState, SabberStoneAction, object, SabberStoneAction>(MinimumVisitThresholdForSelection, nodeEvaluation);
            Builder.EvaluationStrategy = sabberStoneStateEvaluation;
            Builder.Iterations = EnsembleSize > 0 ? Iterations / EnsembleSize : Iterations; // Note: Integer division by design.
            Builder.BackPropagationStrategy = new EvaluateOnceAndColorBackPropagation<List<SabberStoneAction>, SabberStoneState, SabberStoneAction, object, SabberStoneAction>();
            Builder.FinalNodeSelectionStrategy = new BestRatioFinalNodeSelection<List<SabberStoneAction>, SabberStoneState, SabberStoneAction, object, SabberStoneAction>();
            Builder.SolutionStrategy = new SolutionStrategySabberStone(HierarchicalExpansion, nodeEvaluation);
            Builder.PlayoutStrategy = Playout;
        }

        #endregion

        #region Public Methods

        #region ISabberStoneBot

        /// <inheritdoc />
        /// <summary>
        /// Requests the bot to return a SabberStoneAction based on the current SabberStoneState.
        /// </summary>
        /// <param name="state">The current game state.</param>
        /// <returns>SabberStoneAction that was voted as the best option by the ensemble.</returns>
        public SabberStoneAction Act(SabberStoneState state) {
            var timer = System.Diagnostics.Stopwatch.StartNew();
            var gameState = (SabberStoneState) state.Copy();

            if (_debug) Console.WriteLine();
            if (_debug) Console.WriteLine(Name());
            if (_debug) Console.WriteLine($"Starting a MCTS search in turn {(gameState.Game.Turn + 1) / 2}");

            // Check if the task statistics in the searcher should be reset
            if(!RetainTaskStatistics) Searcher.ResetTaskStatistics();

            // Setup and start the ensemble-search
            EnsembleSolutions = new List<SabberStoneAction>();
            var search = (MCTS<List<SabberStoneAction>, SabberStoneState, SabberStoneAction, object, SabberStoneAction>)Builder.Build();
            var context = SearchContext<List<SabberStoneAction>, SabberStoneState, SabberStoneAction, object, SabberStoneAction>.GameSearchSetup(GameLogic, EnsembleSolutions, gameState, null, search);
            Ensemble.EnsembleSearch(context, Searcher.Search, EnsembleSize);
            
            // Determine the best tasks to play based on the ensemble search, or just take the one in case of a single search.
            var solution = EnsembleSize > 1 ? Searcher.DetermineBestTasks(state) : EnsembleSolutions.First();

            var time = timer.ElapsedMilliseconds;
            if (_debug) Console.WriteLine();
            if (_debug) Console.WriteLine($"MCTS returned with solution: {solution}");
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

        /// <summary>
        /// Returns the bot's name.
        /// </summary>
        /// <returns>String representing the bot's name.</returns>
        public string Name() {
            var he = HierarchicalExpansion ? "_HE" : "";
            var pi = PerfectInformation ? "_PI" : "";
            var rts = RetainTaskStatistics ? "_RTS" : "";
            return $"{BOT_NAME}_{Iterations}it_es{EnsembleSize}{he}_{MinimumVisitThresholdForExpansion}mve_{MinimumVisitThresholdForSelection}mvs_{PlayoutTurnCutoff}tc_p{MyPlayoutBot.Name()}_op{OpponentPlayoutBot.Name()}{pi}{rts}";
        }

        /// <summary>
        /// Returns the player's ID.
        /// </summary>
        /// <returns>Integer representing the player's ID.</returns>
        public int PlayerID() {
            return Player.Id;
        }

        /// <summary>
        /// Sets the Controller that the bot represents within a SabberStone Game.
        /// </summary>
        /// <param name="controller">This bot's Controller.</param>
        public void SetController(Controller controller) {
            Player = controller;
        }

        #endregion

        #endregion

    }
}
