using System;
using System.Collections.Generic;
using System.Linq;
using AVThesis.Datastructures;
using AVThesis.Game;
using AVThesis.SabberStone.Strategies;
using AVThesis.Search;
using AVThesis.Search.Tree;
using AVThesis.Search.Tree.MCTS;
using SabberStoneCore.Model;
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
        private const int PLAYOUT_TURN_CUTOFF = 2;
        private const string BOT_NAME = "MCTSBot";
        private readonly Random _rng = new Random();
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
        public IGoalStrategy<object, SabberStoneState, SabberStoneAction, object, SabberStoneAction> Goal { get; set; }

        /// <summary>
        /// The game specific logic required for searching through SabberStoneStates and SabberStoneActions
        /// </summary>
        public IGameLogic<object, SabberStoneState, SabberStoneAction, object, SabberStoneAction, SabberStoneAction> GameLogic { get; set; }

        /// <summary>
        /// The strategy used to play out a game in simulation.
        /// </summary>
        public IPlayoutStrategy<object, SabberStoneState, SabberStoneAction, object, SabberStoneAction> Playout { get; set; }

        /// <summary>
        /// The Monte Carlo Tree Search builder that creates a search-setup ready to use.
        /// </summary>
        public MCTSBuilder<object, SabberStoneState, SabberStoneAction, object, SabberStoneAction> Builder { get; set; }

        /// <summary>
        /// Whether or not to use Hierarchical Expansion during the search.
        /// </summary>
        public bool HierarchicalExpansion { get; set; }

        /// <summary>
        /// Whether or not this bot is allowed perfect information about the game state (i.e. no obfuscation and therefore no determinisation).
        /// </summary>
        public bool PerfectInformation { get; set; }

        /// <summary>
        /// The amount of determinisations the search should use.
        /// </summary>
        public int Determinisations { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructs a new instance of MCTSBot with a <see cref="Controller"/> representing the player.
        /// </summary>
        /// <param name="player">The player.</param>
        /// <param name="hierarchicalExpansion">[Optional] Whether or not to use Hierarchical Expansion. Default value is false.</param>
        /// <param name="allowPerfectInformation">[Optional] Whether or not this bot is allowed perfect information about the game state (i.e. no obfuscation and therefore no determinisation). Default value is false.</param>
        /// <param name="determinisations">[Optional] The amount of determinisations to use. Default value is 1.</param>
        /// <param name="debugInfoToConsole">[Optional] Whether or not to write debug information to the console. Default value is false.</param>
        public MCTSBot(Controller player, bool hierarchicalExpansion = false, bool allowPerfectInformation = false, int determinisations = 1, bool debugInfoToConsole = false) : this(hierarchicalExpansion, allowPerfectInformation, determinisations, debugInfoToConsole) {
            Player = player;
        }

        /// <summary>
        /// Constructs a new instance of MCTSBot with default strategies.
        /// </summary>
        /// <param name="hierarchicalExpansion">[Optional] Whether or not to use Hierarchical Expansion. Default value is false.</param>
        /// <param name="allowPerfectInformation">[Optional] Whether or not this bot is allowed perfect information about the game state (i.e. no obfuscation and therefore no determinisation). Default value is false.</param>
        /// <param name="determinisations">[Optional] The amount of determinisations to use. Default value is 1.</param>
        /// <param name="debugInfoToConsole">[Optional] Whether or not to write debug information to the console. Default value is false.</param>
        public MCTSBot(bool hierarchicalExpansion = false, bool allowPerfectInformation = false, int determinisations = 1, bool debugInfoToConsole = false) {
            HierarchicalExpansion = hierarchicalExpansion;
            PerfectInformation = allowPerfectInformation;
            Determinisations = determinisations;
            _debug = debugInfoToConsole;

            // Simulation will be handled by the Playout.
            var sabberStoneStateEvaluation = new EvaluationStrategyHearthStone();
            var playout = new PlayoutStrategySabberStone();
            MyPlayoutBot = new MASTPlayoutBot(Player, MASTPlayoutBot.SelectionType.EGreedy, sabberStoneStateEvaluation, playout);
            playout.AddPlayoutBot(Player.Id, MyPlayoutBot);
            OpponentPlayoutBot = new RandomBot(Player.Opponent);
            playout.AddPlayoutBot(Player.Opponent.Id, OpponentPlayoutBot);
            Playout = playout;

            // We'll be cutting off the simulations after X turns, using a GoalStrategy.
            Goal = new GoalStrategyTurnCutoff(PLAYOUT_TURN_CUTOFF);

            // Expansion, Application and Goal will be handled by the GameLogic.
            GameLogic = new SabberStoneGameLogic(HierarchicalExpansion, Goal);

            // Create the INodeEvaluation strategy used in the selection phase.
            var nodeEvaluation = new ScoreUCB<SabberStoneState, SabberStoneAction>(UCT_C_CONSTANT_DEFAULT);

            // Build MCTS
            Builder = MCTS<object, SabberStoneState, SabberStoneAction, object, SabberStoneAction>.Builder();
            Builder.ExpansionStrategy = new MinimumTExpansion<object, SabberStoneState, SabberStoneAction, object, SabberStoneAction>(MIN_T_VISIT_THRESHOLD_FOR_EXPANSION);
            Builder.SelectionStrategy = new BestNodeSelection<object, SabberStoneState, SabberStoneAction, object, SabberStoneAction>(SELECTION_VISIT_MINIMUM_FOR_EVALUATION, nodeEvaluation);
            Builder.EvaluationStrategy = sabberStoneStateEvaluation;
            Builder.Iterations = Determinisations > 0 ? MCTS_NUMBER_OF_ITERATIONS / Determinisations : MCTS_NUMBER_OF_ITERATIONS; // Note: Integer division by design.
            Builder.BackPropagationStrategy = new EvaluateOnceAndColorBackPropagation<object, SabberStoneState, SabberStoneAction, object, SabberStoneAction>();
            Builder.FinalNodeSelectionStrategy = new BestRatioFinalNodeSelection<object, SabberStoneState, SabberStoneAction, object, SabberStoneAction>();
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
            var search = (MCTS<object, SabberStoneState, SabberStoneAction, object, SabberStoneAction>)Builder.Build();
            var context = SearchContext<object, SabberStoneState, SabberStoneAction, object, SabberStoneAction>.GameSearchSetup(GameLogic, null, state, null, search);

            // Execute the search
            context.Execute();

            // Check if the search was successful
            if (context.Status != SearchContext<object, SabberStoneState, SabberStoneAction, object, SabberStoneAction>.SearchStatus.Success) {
                // TODO in case of search failure: throw exception, or print error.
                return new Tuple<SabberStoneAction, List<Tuple<SabberStonePlayerTask, double>>>(SabberStoneAction.CreateNullMove(state.Game.CurrentPlayer), new List<Tuple<SabberStonePlayerTask, double>>());
            }

            var solution = context.Solution;
            // Retrieve the value of the final node from the search, this will be important in the case of multiple determinisations
            var solutionStrat = (SolutionStrategySabberStone) search.SolutionStrategy;
            var taskValues = new List<Tuple<SabberStonePlayerTask, double>>(solutionStrat.TaskValues);
            // Make sure to clear the values for the next search
            solutionStrat.ClearTaskValues();

            var time = timer.ElapsedMilliseconds;
            if (_debug) Console.WriteLine($"MCTS returned with solution: {solution}");
            if (_debug) Console.WriteLine($"Calculation time was: {time} ms.");

            // Check if the solution is a complete action.
            if (!solution.IsComplete()) {
                // Otherwise add an End-Turn task before returning.
                if (_debug) Console.WriteLine("Solution was an incomplete action; adding End-Turn task.");
                solution.Tasks.Add((SabberStonePlayerTask)EndTurnTask.Any(Player));
            }

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

        #endregion

        #region Public Methods

        #region ISabberStoneBot

        /// <inheritdoc />
        /// <summary>
        /// Requests the bot to return a SabberStoneAction based on the current SabberStoneState.
        /// </summary>
        /// <param name="state">The current game state.</param>
        /// <returns>SabberStoneAction that was voted as the best option through the Ensemble of determinisations.</returns>
        public SabberStoneAction Act(SabberStoneState state) {
            var timer = System.Diagnostics.Stopwatch.StartNew();
            var gameState = (SabberStoneState) state.Copy();

            if (_debug) Console.WriteLine();
            if (_debug) Console.WriteLine(Name());
            if (_debug) Console.WriteLine($"Starting an ({Determinisations})Ensemble-MCTS-search in turn {(gameState.Game.Turn + 1) / 2}");

            #region Obfuscation

            // In case we need to obfuscate the state ourselves:

            // What we want to do is first check if we know of any cards in the opponent's deck/hand/secret-zone (e.g. quests)
            // Those should not be replaced by random things
            // Create a list of the IDs of those known cards and then obfuscate the state while supplying our list of known cards

            // If we are the starting player, we know the opponent has a Coin
            var knownCards = new List<string>();
            if (gameState.Game.FirstPlayer.Id == gameState.CurrentPlayer()) {
                knownCards.Add("GAME_005");
            }

            if (!PerfectInformation) {
                gameState.Obfuscate(gameState.Game.CurrentOpponent.Id, knownCards);
            }

            // We can get the play history of our opponent (filter out Coin because it never starts in a deck)
            var opponentHistory = gameState.Game.CurrentOpponent.PlayHistory;
            var playedIds = opponentHistory.Where(i => i.SourceCard.Id != "GAME_005").Select(i => i.SourceCard.Id)
                .ToList();
            // TODO try to use these played cards to determine if anything was revealed so we know about it

            #endregion

            //Once the state is correctly obfuscated we can setup a search for each determinisation we are supposed to do

            // Keep track of the solution from each determinisation
            var solutions = new List<SabberStoneAction>();
            var taskValues = new Dictionary<int, PlayerTaskStatistics>();
            for (int i = 0; i < Determinisations; i++) {

                // Copy the state before changing things
                var stateCopy = (SabberStoneState)gameState.Copy();

                #region Determinisation

                if (!PerfectInformation) {
                    // Try to predict / select what deck the opponent is playing
                    var deckDictionary = Decks.AllDecks();
                    var possibleDecks = new List<List<Card>>();
                    foreach (var item in deckDictionary) {
                        var deckIds = Decks.CardIDs(item.Value);
                        // A deck can match if all of the cards we have seen played are present
                        if (playedIds.All(j => deckIds.Contains(j))) {
                            possibleDecks.Add(item.Value);
                        }
                    }

                    List<Card> selectedDeck;
                    // If only one deck matches, assume that is the correct deck
                    if (possibleDecks.Count == 1)
                        selectedDeck = possibleDecks.First();
                    // If we can't,
                    // either just assume the opponent is playing a pre-set dummy deck
                    // or select one of the possible decks at random
                    else selectedDeck = possibleDecks.RandomElementOrDefault();

                    // Determinise the opponent's cards.
                    stateCopy.Determinise(new List<string>(knownCards), selectedDeck, _rng);
                }

                #endregion

                // Run a search and add the result to the collection of possible solutions
                var searchResult = Search(stateCopy);
                solutions.Add(searchResult.Item1);

                // Also record data for each individual task
                foreach (var tuple in searchResult.Item2) {
                    var taskHash = tuple.Item1.GetHashCode();

                    if (!taskValues.ContainsKey(taskHash)) taskValues.Add(taskHash, new PlayerTaskStatistics(tuple.Item1, tuple.Item2));
                    else taskValues[taskHash].AddValue(tuple.Item2);
                }
            }

            // We use the Root Parallelisation technique when there are multiple determinisations
            var solution = Determinisations > 1 ? DetermineBestTasks(state, taskValues) : solutions.RandomElementOrDefault();

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
            return $"{BOT_NAME}_{Builder.Iterations}it_{Determinisations}det{he}{pi}_p{MyPlayoutBot.Name()}_op{OpponentPlayoutBot.Name()}";
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
