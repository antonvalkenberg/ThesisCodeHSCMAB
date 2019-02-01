using System;
using System.Collections.Generic;
using System.Linq;
using AVThesis.Datastructures;
using AVThesis.Game;
using AVThesis.SabberStone.Strategies;
using AVThesis.Search;
using AVThesis.Search.Tree;
using AVThesis.Search.Tree.MCTS;
using SabberStoneCore.Model.Entities;
using SabberStoneCore.Tasks;
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

        private const int MCTS_NUMBER_OF_ITERATIONS = 10000;
        private const int MIN_T_VISIT_THRESHOLD_FOR_EXPANSION = 20;
        private const int SELECTION_VISIT_MINIMUM_FOR_EVALUATION = 50;
        private const double UCT_C_CONSTANT_DEFAULT = 0.1;
        private const int PLAYOUT_TURN_CUTOFF = 3;
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
        /// The type of selection strategy used by the M.A.S.T. playout.
        /// </summary>
        public MASTPlayoutBot.SelectionType MASTSelectionType { get; set; }

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
        /// <param name="debugInfoToConsole">[Optional] Whether or not to write debug information to the console. Default value is false.</param>
        public MCTSBot(Controller player, bool hierarchicalExpansion = true, bool allowPerfectInformation = false, int ensembleSize = 1, MASTPlayoutBot.SelectionType mastSelectionType = MASTPlayoutBot.SelectionType.EGreedy, bool debugInfoToConsole = false)
            : this(hierarchicalExpansion, allowPerfectInformation, ensembleSize, mastSelectionType, debugInfoToConsole) {
            Player = player;

            // Set the playout bots correctly if we are using PlayoutStrategySabberStone
            if (Playout is PlayoutStrategySabberStone playout) {
                MyPlayoutBot.SetController(Player);
                playout.AddPlayoutBot(Player.Id, MyPlayoutBot);
                OpponentPlayoutBot.SetController(Player.Opponent);
                playout.AddPlayoutBot(Player.Id, MyPlayoutBot);
            }
        }

        /// <summary>
        /// Constructs a new instance of MCTSBot with default strategies.
        /// </summary>
        /// <param name="hierarchicalExpansion">[Optional] Whether or not to use Hierarchical Expansion. Default value is true.</param>
        /// <param name="allowPerfectInformation">[Optional] Whether or not this bot is allowed perfect information about the game state (i.e. no obfuscation and therefore no determinisation). Default value is false.</param>
        /// <param name="ensembleSize">[Optional] The size of the ensemble to use. Default value is 1.</param>
        /// <param name="mastSelectionType">[Optional] The type of selection strategy used by the M.A.S.T. playout. Default value is <see cref="MASTPlayoutBot.SelectionType.EGreedy"/>.</param>
        /// <param name="debugInfoToConsole">[Optional] Whether or not to write debug information to the console. Default value is false.</param>
        public MCTSBot(bool hierarchicalExpansion = true, bool allowPerfectInformation = false, int ensembleSize = 1, MASTPlayoutBot.SelectionType mastSelectionType = MASTPlayoutBot.SelectionType.EGreedy, bool debugInfoToConsole = false) {
            HierarchicalExpansion = hierarchicalExpansion;
            PerfectInformation = allowPerfectInformation;
            EnsembleSize = ensembleSize;
            MASTSelectionType = mastSelectionType;
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
            Goal = new GoalStrategyTurnCutoff(PLAYOUT_TURN_CUTOFF);

            // Expansion, Application and Goal will be handled by the GameLogic.
            GameLogic = new SabberStoneGameLogic(HierarchicalExpansion, Goal);

            // Create the INodeEvaluation strategy used in the selection phase.
            var nodeEvaluation = new ScoreUCB<SabberStoneState, SabberStoneAction>(UCT_C_CONSTANT_DEFAULT);

            // Build MCTS
            Builder = MCTS<List<SabberStoneAction>, SabberStoneState, SabberStoneAction, object, SabberStoneAction>.Builder();
            Builder.ExpansionStrategy = new MinimumTExpansion<List<SabberStoneAction>, SabberStoneState, SabberStoneAction, object, SabberStoneAction>(MIN_T_VISIT_THRESHOLD_FOR_EXPANSION);
            Builder.SelectionStrategy = new BestNodeSelection<List<SabberStoneAction>, SabberStoneState, SabberStoneAction, object, SabberStoneAction>(SELECTION_VISIT_MINIMUM_FOR_EVALUATION, nodeEvaluation);
            Builder.EvaluationStrategy = sabberStoneStateEvaluation;
            Builder.Iterations = EnsembleSize > 0 ? MCTS_NUMBER_OF_ITERATIONS / EnsembleSize : MCTS_NUMBER_OF_ITERATIONS; // Note: Integer division by design.
            Builder.BackPropagationStrategy = new EvaluateOnceAndColorBackPropagation<List<SabberStoneAction>, SabberStoneState, SabberStoneAction, object, SabberStoneAction>();
            Builder.FinalNodeSelectionStrategy = new BestRatioFinalNodeSelection<List<SabberStoneAction>, SabberStoneState, SabberStoneAction, object, SabberStoneAction>();
            Builder.SolutionStrategy = new SolutionStrategySabberStone(HierarchicalExpansion, nodeEvaluation);
            Builder.PlayoutStrategy = Playout;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Runs a single search.
        /// </summary>
        /// <param name="state">The state to search.</param>
        /// <returns>Tuple of the selected SabberStoneAction and a list of values for the individual PlayerTasks.</returns>
        private Tuple<SabberStoneAction, List<Tuple<SabberStonePlayerTask, double>>> Search(SabberStoneState state) {
            var timer = System.Diagnostics.Stopwatch.StartNew();
            if (_debug) Console.WriteLine();

            // Setup a new search with the current state as source.
            var search = (MCTS<List<SabberStoneAction>, SabberStoneState, SabberStoneAction, object, SabberStoneAction>)Builder.Build();
            var context = SearchContext<List<SabberStoneAction>, SabberStoneState, SabberStoneAction, object, SabberStoneAction>.GameSearchSetup(GameLogic, null, state, null, search);

            // Execute the search
            context.Execute();

            // Check if the search was successful
            if (context.Status != SearchContext<List<SabberStoneAction>, SabberStoneState, SabberStoneAction, object, SabberStoneAction>.SearchStatus.Success) {
                // TODO in case of search failure: throw exception, or print error.
                return new Tuple<SabberStoneAction, List<Tuple<SabberStonePlayerTask, double>>>(SabberStoneAction.CreateNullMove(state.Game.CurrentPlayer), new List<Tuple<SabberStonePlayerTask, double>>());
            }

            var solution = context.Solution;
            // Retrieve the value of the final node from the search, this will be important in the case of an ensemble-search
            var solutionStrategy = (SolutionStrategySabberStone) search.SolutionStrategy;
            var taskValues = new List<Tuple<SabberStonePlayerTask, double>>(solutionStrategy.TaskValues);
            // Make sure to clear the values for the next search
            solutionStrategy.ClearTaskValues();

            var time = timer.ElapsedMilliseconds;
            if (_debug) Console.WriteLine($"MCTS returned with solution: {solution}");
            if (_debug) Console.WriteLine($"Calculation time was: {time} ms.");

            // Check if the solution is a complete action.
            if (solution.IsComplete()) return new Tuple<SabberStoneAction, List<Tuple<SabberStonePlayerTask, double>>>(solution, taskValues);
            // Otherwise add an End-Turn task before returning.
            if (_debug) Console.WriteLine("Solution was an incomplete action; adding End-Turn task.");
            solution.Tasks.Add((SabberStonePlayerTask)EndTurnTask.Any(Player));
            return new Tuple<SabberStoneAction, List<Tuple<SabberStonePlayerTask, double>>>(solution, taskValues);
        }

        /// <summary>
        /// Determines the best tasks for the game state based on the provided statistics and creates a <see cref="SabberStoneAction"/> from them.
        /// </summary>
        /// <param name="state">The game state to create the best action for.</param>
        /// <param name="taskStatistics">The statistics for individual tasks.</param>
        /// <returns><see cref="SabberStoneAction"/> created from the best individual tasks available in the provided state.</returns>
        private static SabberStoneAction DetermineBestTasks(SabberStoneState state, Dictionary<int, PlayerTaskStatistics> taskStatistics) {
            // Clone game so that we can process the selected tasks and get an updated options list.
            var clonedGame = state.Game.Clone();
            var clonedPlayer = clonedGame.CurrentPlayer;

            // We have to determine which tasks are the best to execute in this state, based on the provided values of the MCTS search.
            // So we'll check the statistics table for the highest value among tasks that are currently available in the state.
            // This continues until the end-turn task is selected.
            var action = new SabberStoneAction();
            KeyValuePair<int, PlayerTaskStatistics> bestTask;
            do {
                // Get the available options in this state and find which tasks we have statistics on.
                var availableTasks = clonedPlayer.Options().Cast<SabberStonePlayerTask>().Select(i =>i.GetHashCode());
                bestTask = taskStatistics.Where(i => availableTasks.Contains(i.Key)).OrderByDescending(i => i.Value.AverageValue()).FirstOrDefault();

                // If we can't find any task, stop.
                if (bestTask.IsDefault()) break;
                
                // If we found a task, add it to the Action and process it to progress the game.
                var task = bestTask.Value.Task;
                action.AddTask(task);
                clonedGame.Process(task.Task);

                // Continue while it is still our turn and we haven't yet selected to end the turn.
            } while (clonedGame.CurrentPlayer.Id == clonedPlayer.Id && bestTask.Value.Task.Task.PlayerTaskType != PlayerTaskType.END_TURN);

            // Return the created action consisting of the best action available at each point.
            return action;
        }

        private SabberStoneAction Search(SearchContext<List<SabberStoneAction>, SabberStoneState, SabberStoneAction, object, SabberStoneAction> context, SabberStoneState state) {
            throw new NotImplementedException();
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
            if (_debug) Console.WriteLine($"Starting an ({EnsembleSize})Ensemble-MCTS-search in turn {(gameState.Game.Turn + 1) / 2}");

            // TODO MCTS-bot -> setup Ensemble search
            var search = (MCTS<List<SabberStoneAction>, SabberStoneState, SabberStoneAction, object, SabberStoneAction>)Builder.Build();
            var context = SearchContext<List<SabberStoneAction>, SabberStoneState, SabberStoneAction, object, SabberStoneAction>.GameSearchSetup(GameLogic, null, gameState, null, search);

            Ensemble.EnsembleSearch(context, Search, EnsembleSize);

            // We use the Root Parallelisation technique when an ensemble search is used
            SabberStoneAction solution = null;//EnsembleSize > 1 ? DetermineBestTasks(state, taskValues) : solutions.RandomElementOrDefault();

            var time = timer.ElapsedMilliseconds;
            if (_debug) Console.WriteLine();
            if (_debug) Console.WriteLine($"Ensemble-MCTS returned with solution: {solution}");
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
            return $"{BOT_NAME}_{Builder.Iterations}it_es{EnsembleSize}{he}{pi}_p{MyPlayoutBot.Name()}_op{OpponentPlayoutBot.Name()}";
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
