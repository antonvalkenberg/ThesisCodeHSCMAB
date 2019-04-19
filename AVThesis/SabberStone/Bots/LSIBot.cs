using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using AVThesis.Datastructures;
using AVThesis.Enums;
using AVThesis.Game;
using AVThesis.SabberStone.Strategies;
using AVThesis.Search;
using AVThesis.Search.LSI;
using AVThesis.Search.Tree;
using SabberStoneCore.Model.Entities;
using SabberStoneCore.Tasks;
using SabberStoneCore.Tasks.PlayerTasks;

/// <summary>
/// Written by A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by G.J. Roelofs and T. Aliyev.
/// </summary>
namespace AVThesis.SabberStone.Bots {

    /// <summary>
    /// A bot that plays Hearthstone using Linear Side Information to find its best move.
    /// </summary>
    public class LSIBot : ISabberStoneBot {

        #region Enums

        /// <summary>
        /// Enumeration of the types of strategies used to estimate the budget for LSI.
        /// </summary>
        public enum BudgetEstimationType {
            AverageSampleTime, PreviousSearchAverage
        }

        #endregion

        #region Inner Classes

        private class SabberStoneSideInformationStrategy : ISideInformationStrategy<List<SabberStoneAction>, SabberStoneState, SabberStoneAction, object, SabberStoneAction, OddmentTable<SabberStonePlayerTask>> {

            #region Properties

            /// <summary>
            /// The bot that is used during the playouts.
            /// </summary>
            private RandomBot PlayoutBot { get; }
            
            /// <summary>
            /// The strategy used to play out a game in simulation.
            /// </summary>
            private IPlayoutStrategy<List<SabberStoneAction>, SabberStoneState, SabberStoneAction, object, SabberStoneAction> Playout { get; }

            /// <summary>
            /// The evaluation strategy for determining the value of samples.
            /// </summary>
            private IStateEvaluation<List<SabberStoneAction>, SabberStoneState, SabberStoneAction, object, SabberStoneAction, TreeSearchNode<SabberStoneState, SabberStoneAction>> Evaluation { get; }

            /// <summary>
            /// The game specific logic required for searching through SabberStoneStates and SabberStoneActions.
            /// </summary>
            private IGameLogic<List<SabberStoneAction>, SabberStoneState, SabberStoneAction, object, SabberStoneAction, SabberStoneAction> GameLogic { get; }

            #endregion

            #region Constructor

            /// <summary>
            /// Creates a new instance.
            /// </summary>
            /// <param name="playout">The strategy used to play out a game in simulation.</param>
            /// <param name="evaluation">The evaluation strategy for determining the value of samples.</param>
            /// <param name="gameLogic">The game specific logic required for searching through SabberStoneStates and SabberStoneActions.</param>
            public SabberStoneSideInformationStrategy(IPlayoutStrategy<List<SabberStoneAction>, SabberStoneState, SabberStoneAction, object, SabberStoneAction> playout, IStateEvaluation<List<SabberStoneAction>, SabberStoneState, SabberStoneAction, object, SabberStoneAction, TreeSearchNode<SabberStoneState, SabberStoneAction>> evaluation, IGameLogic<List<SabberStoneAction>, SabberStoneState, SabberStoneAction, object, SabberStoneAction, SabberStoneAction> gameLogic) {
                PlayoutBot = new RandomBot();
                Playout = playout;
                Evaluation = evaluation;
                GameLogic = gameLogic;
            }

            #endregion

            #region Public Methods

            /// <inheritdoc />
            public OddmentTable<SabberStonePlayerTask> Create(SearchContext<List<SabberStoneAction>, SabberStoneState, SabberStoneAction, object, SabberStoneAction> context, int samplesForGeneration) {

                // So we have an issue here, because it's Hearthstone we don't know in advance how many dimensions we have.
                //      -> We can just evenly distribute budget over the currently available dimensions
                //      -- Reason is that some dimensions would be `locked' until after a specific dimensions is explored
                //      -> Another option is to expand all possible sequences, this might be too complex though...

                // In Hearthstone we don't really have multiple available actions per dimension
                // It's more like you either play/attack or not
                // Although each minion can have multiple choices on what to attack

                // We can still try to distillate individual actions
                // It might be best to keep track of these through a dictionary/array
                //      So we'd randomly choose actions and simulate when `end-turn' is chosen
                //      And then update the value of any selected PlayerTask

                // I guess use OddmentTable again?

                var table = new Dictionary<SabberStonePlayerTask, double>(PlayerTaskComparer.Comparer);

                // So, we have a number of samples to use
                // For each of those, generate a random SabberStoneAction and playout
                for (var i = 0; i < samplesForGeneration; i++) {

                    var action = PlayoutBot.CreateRandomAction(context.Source);

                    var newState = GameLogic.Apply(context, (SabberStoneState)context.Source.Copy(), action);

                    var endState = Playout.Playout(context, newState);

                    var value = Evaluation.Evaluate(context, null, endState);

                    foreach (var task in action.Tasks) {
                        if (!table.ContainsKey(task)) table.Add(task, 0);
                        table[task] += value;
                    }
                }

                // Create the Oddment table
                var oddmentTable = new OddmentTable<SabberStonePlayerTask>();
                foreach (var kvPair in table) {
                    oddmentTable.Add(kvPair.Key, kvPair.Value, recalculate: false);
                }

                oddmentTable.Recalculate();

                return oddmentTable;
            }

            #endregion

        }

        private class SabberStoneLSISamplingStrategy : ILSISamplingStrategy<SabberStoneState, SabberStoneAction, OddmentTable<SabberStonePlayerTask>> {
            
            #region Properties

            /// <summary>
            /// The game specific logic required for searching through SabberStoneStates and SabberStoneActions.
            /// </summary>
            private IGameLogic<List<SabberStoneAction>, SabberStoneState, SabberStoneAction, object, SabberStoneAction, SabberStoneAction> GameLogic { get; }

            #endregion

            #region Constructor

            /// <summary>
            /// Constructs a new instance.
            /// </summary>
            /// <param name="gameLogic">The game specific logic required for searching through SabberStoneStates and SabberStoneActions.</param>
            public SabberStoneLSISamplingStrategy(IGameLogic<List<SabberStoneAction>, SabberStoneState, SabberStoneAction, object, SabberStoneAction, SabberStoneAction> gameLogic) {
                GameLogic = gameLogic;
            }

            #endregion

            #region Private Methods

            /// <summary>
            /// Expands a specific state and returns all available SabberStonePlayerTasks.
            /// Note: it is assumed that the expansion is performed Hierarchically and therefore only contains single SabberStonePlayerTask SabberStoneActions.
            /// </summary>
            /// <param name="state">The game state.</param>
            /// <returns>Collection of SabberStonePlayerTasks that are available in the provided state.</returns>
            private List<SabberStonePlayerTask> GetAvailablePlayerTasks(SabberStoneState state) {
                return GameLogic.Expand(null, state).Select(expandedAction => expandedAction.Tasks.First()).ToList();
            }

            #endregion
            
            #region Public Methods

            /// <inheritdoc />
            public SabberStoneAction Sample(SabberStoneState state, OddmentTable<SabberStonePlayerTask> sideInformation) {
                var copyState = (SabberStoneState)state.Copy();
                var availableTasks = GetAvailablePlayerTasks(copyState);
                var action = new SabberStoneAction();
                var tries = 0;

                // Keep sampling tasks while we have not passed the turn yet and there are more tasks available than only EndTurn or HeroPower, of if we haven't generated a suitable task in 100 tries
                while (!action.IsComplete() && availableTasks.Any(i => i.Task.PlayerTaskType != PlayerTaskType.END_TURN && i.Task.PlayerTaskType != PlayerTaskType.HERO_POWER) && tries < 100) {
                    // Sample a task from the OddmentTable
                    var task = sideInformation.Next();
                    // Check if the task is available in the current state
                    if (!availableTasks.Contains(task, PlayerTaskComparer.Comparer)) {
                        tries++;
                        continue;
                    }

                    tries = 0;
                    action.AddTask(task);
                    copyState.Game.Process(task.Task);
                    availableTasks = GetAvailablePlayerTasks(copyState);
                }

                if (action.IsComplete()) return action;

                // If hero power is available, add it
                if (availableTasks.Any(i => i.Task.PlayerTaskType == PlayerTaskType.HERO_POWER))
                   action.AddTask(availableTasks.First(i => i.Task.PlayerTaskType == PlayerTaskType.HERO_POWER));
                
                // If the action is not complete yet, add EndTurn
                action.AddTask((SabberStonePlayerTask) EndTurnTask.Any(state.Game.CurrentPlayer));

                return action;
            }

            /// <inheritdoc />
            public SabberStoneAction Sample(SabberStoneState state) {
                return SabberStoneAction.CreateNullMove(state.CurrentPlayer() == state.Player1.Id ? state.Player1 : state.Player2);
            }

            #endregion
            
        }

        private class AverageSampleTimeBudgetEstimationStrategy : IBudgetEstimationStrategy<List<SabberStoneAction>, SabberStoneState, SabberStoneAction, object, SabberStoneAction> {

            #region Constants

            /// <summary>
            /// The amount of playouts to run when determining sample sizes when on a time budget.
            /// </summary>
            private const int TIME_BUDGET_TEST_PLAYOUTS = 25;

            /// <summary>
            /// The factor with which to multiply a single sample's time when determining sample sizes on a time budget.
            /// </summary>
            private const double TIME_BUDGET_SAFETY_MARGIN = 1.5;

            #endregion

            #region Properties

            /// <summary>
            /// The type of budget that is being used by the LSI search.
            /// </summary>
            private BudgetType BudgetType { get; }

            /// <summary>
            /// The amount of budget that the search is allowed to expend.
            /// Note: what this represents is relative to the type of budget.
            /// </summary>
            private long BudgetAllowance { get; }

            /// <summary>
            /// The percentage of the budget that should be spent during the generation phase.
            /// </summary>
            private double GenerationBudgetPercentage { get; }

            /// <summary>
            /// The strategy used to play out a game in simulation.
            /// </summary>
            private PlayoutStrategySabberStone Playout { get; }

            #endregion

            #region Constructors

            /// <summary>
            /// Constructs a new instance of AverageSampleTimeBudgetEstimationStrategy.
            /// </summary>
            /// <param name="budgetType">The type of budget that is being used by the LSI search.</param>
            /// <param name="budgetAllowance">The amount of budget that the search is allowed to expend.</param>
            /// <param name="generationBudgetPercentage">The percentage of the budget that should be spent during the generation phase.</param>
            /// <param name="playout">The strategy used to play out a game in simulation.</param>
            public AverageSampleTimeBudgetEstimationStrategy(BudgetType budgetType, long budgetAllowance, double generationBudgetPercentage, PlayoutStrategySabberStone playout) {
                BudgetType = budgetType;
                BudgetAllowance = budgetAllowance;
                GenerationBudgetPercentage = generationBudgetPercentage;
                Playout = playout;
            }

            #endregion

            #region Public Methods

            /// <inheritdoc />
            public void DetermineSampleSizes(SearchContext<List<SabberStoneAction>, SabberStoneState, SabberStoneAction, object, SabberStoneAction> context, out int generationSamples, out int evaluationSamples) {
                long estimatedSamples;

                switch (BudgetType) {
                    case BudgetType.Iterations:
                        // If we're running on an iterations budget, we know exactly how many we can spend.
                        estimatedSamples = BudgetAllowance;
                        break;
                    case BudgetType.Time:
                        // If we're running on a time budget, estimate the duration of a single sample by running some test samples.
                        estimatedSamples = EstimateBudgetWithTestPlayouts(context, Playout, BudgetAllowance);
                        break;
                    default:
                        throw new InvalidEnumArgumentException($"BudgetType `{BudgetType}' is not supported.");
                }

                generationSamples = (int)(estimatedSamples * GenerationBudgetPercentage);
                evaluationSamples = (int)(estimatedSamples * (1 - GenerationBudgetPercentage) * Constants.DEFAULT_LSI_EVALUATION_SAMPLES_ADJUSTMENT_FACTOR);
            }

            /// <summary>
            /// Attempts to estimate the amount of samples that can be run within the allowed budget.
            /// </summary>
            /// <param name="context">The search context.</param>
            /// <param name="playout">The simulation strategy being used.</param>
            /// <param name="budgetInMilliseconds">The budget that can be spent on the entire search, specified in milliseconds.</param>
            /// <returns>A number indicating the estimated amount of samples that can be run while remaining with budget.</returns>
            public static long EstimateBudgetWithTestPlayouts(SearchContext<List<SabberStoneAction>, SabberStoneState, SabberStoneAction, object, SabberStoneAction> context, PlayoutStrategySabberStone playout, long budgetInMilliseconds) {
                // Check how long it takes to run X playouts
                var timer = Stopwatch.StartNew();
                for (var i = 0; i < TIME_BUDGET_TEST_PLAYOUTS; i++) {
                    playout.Playout(context, context.Source.Copy());
                }
                timer.Stop();
                // Determine how long a single sample took
                var testDuration = timer.ElapsedMilliseconds;
                var sampleDuration = testDuration / (TIME_BUDGET_TEST_PLAYOUTS * 1.0);
                // Estimate how many samples we can run in the remaining time
                return (long)((budgetInMilliseconds - testDuration) / (sampleDuration * TIME_BUDGET_SAFETY_MARGIN));
            }

            #endregion

        }

        private class PreviousSearchAverageBudgetEstimationStrategy : IBudgetEstimationStrategy<List<SabberStoneAction>, SabberStoneState, SabberStoneAction, object, SabberStoneAction> {
            
            #region Properties

            /// <summary>
            /// The amount of time spent during the last search.
            /// </summary>
            public long PreviousSearchTime { private get; set; }

            /// <summary>
            /// The amount of samples spent during the last search.
            /// </summary>
            public long PreviousSearchIterations { private get; set; }

            /// <summary>
            /// The type of budget that is being used by the LSI search.
            /// </summary>
            private BudgetType BudgetType { get; }

            /// <summary>
            /// The amount of budget that the search is allowed to expend.
            /// Note: what this represents is relative to the type of budget.
            /// </summary>
            private long BudgetAllowance { get; }

            /// <summary>
            /// The percentage of the budget that should be spent during the generation phase.
            /// </summary>
            private double GenerationBudgetPercentage { get; }

            /// <summary>
            /// The strategy used to play out a game in simulation.
            /// </summary>
            private PlayoutStrategySabberStone Playout { get; }

            #endregion

            #region Constructors

            /// <summary>
            /// Constructs a new instance of PreviousSearchAverageBudgetEstimationStrategy.
            /// </summary>
            /// <param name="budgetType">The type of budget that is being used by the LSI search.</param>
            /// <param name="budgetAllowance">The amount of budget that the search is allowed to expend.</param>
            /// <param name="generationBudgetPercentage">The percentage of the budget that should be spent during the generation phase.</param>
            /// <param name="playout">The strategy used to play out a game in simulation.</param>
            public PreviousSearchAverageBudgetEstimationStrategy(BudgetType budgetType, long budgetAllowance, double generationBudgetPercentage, PlayoutStrategySabberStone playout) {
                BudgetType = budgetType;
                BudgetAllowance = budgetAllowance;
                GenerationBudgetPercentage = generationBudgetPercentage;
                Playout = playout;
            }

            #endregion

            #region Public Methods

            /// <inheritdoc />
            public void DetermineSampleSizes(SearchContext<List<SabberStoneAction>, SabberStoneState, SabberStoneAction, object, SabberStoneAction> context, out int generationSamples, out int evaluationSamples) {
                long estimatedSamples;

                switch (BudgetType) {
                    case BudgetType.Iterations:
                        // If we're running on an iterations budget, we know exactly how many we can spend.
                        estimatedSamples = BudgetAllowance;
                        break;
                    case BudgetType.Time:
                        // If we're running on a time budget, we can check what the previous search was able to run.
                        if (PreviousSearchTime > 0 && PreviousSearchIterations > 0) {
                            var sampleDuration = PreviousSearchTime / (PreviousSearchIterations * 1.0);
                            // We can now estimate how many samples go into the budget we are allowed to spend
                            estimatedSamples = (long) (BudgetAllowance / sampleDuration);
                        }
                        else {
                            // If we don't have any statistics on the previous search (maybe cause this is going to be the first)
                            // use an estimation strategy based on a couple playouts
                            estimatedSamples = AverageSampleTimeBudgetEstimationStrategy.EstimateBudgetWithTestPlayouts(context, Playout, BudgetAllowance);
                        }
                        break;
                    default:
                        throw new InvalidEnumArgumentException($"BudgetType `{BudgetType}' is not supported.");
                }

                generationSamples = (int)(estimatedSamples * GenerationBudgetPercentage);
                evaluationSamples = (int)(estimatedSamples * (1 - GenerationBudgetPercentage) * Constants.DEFAULT_LSI_EVALUATION_SAMPLES_ADJUSTMENT_FACTOR);
            }

            #endregion

        }

        #endregion

        #region Constants

        private const string BOT_NAME = "LSIBot";
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
        public IPlayoutBot MyPlayoutBot { get; set; }

        /// <summary>
        /// The bot that is used for the opponent's playouts.
        /// </summary>
        public IPlayoutBot OpponentPlayoutBot { get; set; }

        /// <summary>
        /// The type of playout bot to be used during playouts.
        /// </summary>
        public PlayoutBotType PlayoutBotType { get; set; }

        /// <summary>
        /// The amount of turns after which to stop a simulation.
        /// </summary>
        public int PlayoutTurnCutoff { get; set; }

        /// <summary>
        /// The strategy used to determine if a playout has reached its goal state.
        /// </summary>
        public IGoalStrategy<List<SabberStoneAction>, SabberStoneState, SabberStoneAction, object, SabberStoneAction> Goal { get; set; }

        /// <summary>
        /// The strategy used to play out a game in simulation.
        /// </summary>
        public PlayoutStrategySabberStone Playout { get; set; }

        /// <summary>
        /// The evaluation strategy for determining the value of samples.
        /// </summary>
        public IStateEvaluation<List<SabberStoneAction>, SabberStoneState, SabberStoneAction, object, SabberStoneAction, TreeSearchNode<SabberStoneState, SabberStoneAction>> Evaluation { get; set; }

        /// <summary>
        /// The game specific logic required for searching through SabberStoneStates and SabberStoneActions
        /// </summary>
        public IGameLogic<List<SabberStoneAction>, SabberStoneState, SabberStoneAction, object, SabberStoneAction, SabberStoneAction> GameLogic { get; set; }
        
        /// <summary>
        /// The strategy for creating the side information.
        /// </summary>
        public ISideInformationStrategy<List<SabberStoneAction>, SabberStoneState, SabberStoneAction, object, SabberStoneAction, OddmentTable<SabberStonePlayerTask>> SideInformationStrategy { get; set; }

        /// <summary>
        /// The strategy for sampling actions during the generation phase of LSI.
        /// </summary>
        public ILSISamplingStrategy<SabberStoneState, SabberStoneAction, OddmentTable<SabberStonePlayerTask>> SamplingStrategy { get; set; }

        /// <summary>
        /// The strategy used to estimate the sample sizes for the budget of this bot's LSI search.
        /// </summary>
        public IBudgetEstimationStrategy<List<SabberStoneAction>, SabberStoneState, SabberStoneAction, object, SabberStoneAction> BudgetEstimationStrategy { get; set; }

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
        /// The type of selection strategy used by the M.A.S.T. playout.
        /// </summary>
        public MASTPlayoutBot.SelectionType MASTSelectionType { get; set; }

        /// <summary>
        /// The budget for the amount of samples LSI can use.
        /// </summary>
        public int Samples { get; set; }

        /// <summary>
        /// The total amount of samples spent during the calculation of the latest solution.
        /// </summary>
        public long SamplesSpent { get; set; }

        /// <summary>
        /// The budget for the amount of milliseconds LSI can spend on searching.
        /// </summary>
        public long Time { get; set; }

        /// <summary>
        /// The type of budget that this bot will use.
        /// </summary>
        public BudgetType BudgetType { get; set; }

        /// <summary>
        /// The percentage of the budget that should be spent during the generation phase.
        /// </summary>
        public double GenerationBudgetPercentage { get; set; }

        /// <summary>
        /// The type of budget estimation strategy we'll be using.
        /// </summary>
        public BudgetEstimationType BudgetEstimation { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructs a new instance of LSIBot with a <see cref="Controller"/> representing the player.
        /// </summary>
        /// <param name="player">The player.</param>
        /// <param name="allowPerfectInformation">[Optional] Whether or not this bot is allowed perfect information about the game state (i.e. no obfuscation and therefore no determinisation). Default value is false.</param>
        /// <param name="ensembleSize">[Optional] The size of the ensemble to use. Default value is 1.</param>
        /// <param name="playoutBotType">[Optional] The type of playout bot to be used during playouts. Default value is <see cref="PlayoutBotType.MAST"/>.</param>
        /// <param name="mastSelectionType">[Optional] The type of selection strategy used by the M.A.S.T. playout. Default value is <see cref="MASTPlayoutBot.SelectionType.EGreedy"/>.</param>
        /// <param name="playoutTurnCutoff">[Optional] The amount of turns after which to stop a simulation. Default value is <see cref="Constants.DEFAULT_PLAYOUT_TURN_CUTOFF"/>.</param>
        /// <param name="budgetType">[Optional] The type of budget that this bot will use. Default value is <see cref="BudgetType.Iterations"/>.</param>
        /// <param name="samples">[Optional] The budget for the amount of iterations LSI can use. Default value is <see cref="Constants.DEFAULT_COMPUTATION_ITERATION_BUDGET"/>.</param>
        /// <param name="time">[Optional] The budget for the amount of milliseconds LSI can spend on searching. Default value is <see cref="Constants.DEFAULT_COMPUTATION_TIME_BUDGET"/>.</param>
        /// <param name="generationBudgetPercentage">[Optional] The percentage of the budget that should be spent during the generation phase. Default value is <see cref="Constants.DEFAULT_LSI_BUDGET_GENERATION_PERCENTAGE"/>.</param>
        /// <param name="budgetEstimationType">[Optional] The type of strategy used to estimate the budget for LSI. Default value is <see cref="BudgetEstimationType.AverageSampleTime"/>.</param>
        /// <param name="debugInfoToConsole">[Optional] Whether or not to write debug information to the console. Default value is false.</param>
        public LSIBot(Controller player,
            bool allowPerfectInformation = false,
            int ensembleSize = 1,
            PlayoutBotType playoutBotType = PlayoutBotType.MAST,
            MASTPlayoutBot.SelectionType mastSelectionType = MASTPlayoutBot.SelectionType.EGreedy,
            int playoutTurnCutoff = Constants.DEFAULT_PLAYOUT_TURN_CUTOFF,
            BudgetType budgetType = BudgetType.Iterations,
            int samples = Constants.DEFAULT_COMPUTATION_ITERATION_BUDGET,
            long time = Constants.DEFAULT_COMPUTATION_TIME_BUDGET,
            double generationBudgetPercentage = Constants.DEFAULT_LSI_BUDGET_GENERATION_PERCENTAGE,
            BudgetEstimationType budgetEstimationType = BudgetEstimationType.AverageSampleTime,
            bool debugInfoToConsole = false)
            : this(allowPerfectInformation, ensembleSize, playoutBotType, mastSelectionType, playoutTurnCutoff, budgetType, samples, time, generationBudgetPercentage, budgetEstimationType, debugInfoToConsole) {
            SetController(player);
        }

        /// <summary>
        /// Constructs a new instance of LSIBot with default strategies.
        /// </summary>
        /// <param name="allowPerfectInformation">[Optional] Whether or not this bot is allowed perfect information about the game state (i.e. no obfuscation and therefore no determinisation). Default value is false.</param>
        /// <param name="ensembleSize">[Optional] The size of the ensemble to use. Default value is 1.</param>
        /// <param name="playoutBotType">[Optional] The type of playout bot to be used during playouts. Default value is <see cref="PlayoutBotType.MAST"/>.</param>
        /// <param name="mastSelectionType">[Optional] The type of selection strategy used by the M.A.S.T. playout. Default value is <see cref="MASTPlayoutBot.SelectionType.EGreedy"/>.</param>
        /// <param name="playoutTurnCutoff">[Optional] The amount of turns after which to stop a simulation. Default value is <see cref="Constants.DEFAULT_PLAYOUT_TURN_CUTOFF"/>.</param>
        /// <param name="budgetType">[Optional] The type of budget that this bot will use. Default value is <see cref="BudgetType.Iterations"/>.</param>
        /// <param name="samples">[Optional] The budget for the amount of iterations LSI can use. Default value is <see cref="Constants.DEFAULT_COMPUTATION_ITERATION_BUDGET"/>.</param>
        /// <param name="time">[Optional] The budget for the amount of milliseconds LSI can spend on searching. Default value is <see cref="Constants.DEFAULT_COMPUTATION_TIME_BUDGET"/>.</param>
        /// <param name="generationBudgetPercentage">[Optional] The percentage of the budget that should be spent during the generation phase. Default value is <see cref="Constants.DEFAULT_LSI_BUDGET_GENERATION_PERCENTAGE"/>.</param>
        /// <param name="budgetEstimationType">[Optional] The type of strategy used to estimate the budget for LSI. Default value is <see cref="BudgetEstimationType.AverageSampleTime"/>.</param>
        /// <param name="debugInfoToConsole">[Optional] Whether or not to write debug information to the console. Default value is false.</param>
        public LSIBot(bool allowPerfectInformation = false,
            int ensembleSize = 1,
            PlayoutBotType playoutBotType = PlayoutBotType.MAST,
            MASTPlayoutBot.SelectionType mastSelectionType = MASTPlayoutBot.SelectionType.EGreedy,
            int playoutTurnCutoff = Constants.DEFAULT_PLAYOUT_TURN_CUTOFF,
            BudgetType budgetType = BudgetType.Iterations,
            int samples = Constants.DEFAULT_COMPUTATION_ITERATION_BUDGET,
            long time = Constants.DEFAULT_COMPUTATION_TIME_BUDGET,
            double generationBudgetPercentage = Constants.DEFAULT_LSI_BUDGET_GENERATION_PERCENTAGE,
            BudgetEstimationType budgetEstimationType = BudgetEstimationType.AverageSampleTime,
            bool debugInfoToConsole = false) {
            PerfectInformation = allowPerfectInformation;
            EnsembleSize = ensembleSize;
            PlayoutBotType = playoutBotType;
            MASTSelectionType = mastSelectionType;
            PlayoutTurnCutoff = playoutTurnCutoff;
            BudgetType = budgetType;
            Samples = samples;
            Time = time;
            GenerationBudgetPercentage = generationBudgetPercentage;
            BudgetEstimation = budgetEstimationType;
            _debug = debugInfoToConsole;

            // Create the ensemble search
            Ensemble = new EnsembleStrategySabberStone(enableStateObfuscation: true, enablePerfectInformation: PerfectInformation);

            // Adjust sample sizes for use in the Ensemble
            long budgetAllowance;
            switch (BudgetType) {
                case BudgetType.Iterations:
                    Samples = EnsembleSize > 0 ? Samples / EnsembleSize : Samples; // Note: Integer division by design.
                    budgetAllowance = Samples;
                    break;
                case BudgetType.Time:
                    Time = EnsembleSize > 0 ? Time / EnsembleSize : Time; // Note: Integer division by design.
                    budgetAllowance = Time;
                    break;
                default:
                    throw new InvalidEnumArgumentException($"BudgetType `{BudgetType}' is not supported.");
            }

            // Simulation will be handled by the Playout.
            var sabberStoneStateEvaluation = new EvaluationStrategyHearthStone();
            Playout = new PlayoutStrategySabberStone();

            // Set the playout bots
            switch (PlayoutBotType) {
                case PlayoutBotType.Random:
                    MyPlayoutBot = new RandomBot(filterDuplicatePositionTasks: true);
                    OpponentPlayoutBot = new RandomBot(filterDuplicatePositionTasks: true);
                    break;
                case PlayoutBotType.Heuristic:
                    MyPlayoutBot = new HeuristicBot();
                    OpponentPlayoutBot = new HeuristicBot();
                    break;
                case PlayoutBotType.MAST:
                    MyPlayoutBot = new MASTPlayoutBot(MASTSelectionType, sabberStoneStateEvaluation);
                    OpponentPlayoutBot = new MASTPlayoutBot(MASTSelectionType, sabberStoneStateEvaluation);
                    break;
                default:
                    throw new InvalidEnumArgumentException($"PlayoutBotType `{PlayoutBotType}' is not supported.");
            }

            // Set the budget estimation strategy.
            switch (BudgetEstimation) {
                case BudgetEstimationType.AverageSampleTime:
                    BudgetEstimationStrategy = new AverageSampleTimeBudgetEstimationStrategy(BudgetType, budgetAllowance, GenerationBudgetPercentage, Playout);
                    break;
                case BudgetEstimationType.PreviousSearchAverage:
                    BudgetEstimationStrategy = new PreviousSearchAverageBudgetEstimationStrategy(BudgetType, budgetAllowance, GenerationBudgetPercentage, Playout);
                    break;
                default:
                    throw new InvalidEnumArgumentException($"BudgetEstimationType `{budgetEstimationType}' is not supported.");
            }

            // LSI will need a goal-strategy to determine when a simulation is done
            Goal = new GoalStrategyTurnCutoff(PlayoutTurnCutoff);

            // LSI will need an evaluation-strategy to evaluate the strength of samples
            Evaluation = new EvaluationStrategyHearthStone();

            // Application will be handled by the GameLogic
            // Hierarchical Expansion is set to TRUE because of incremental task validation during action sampling.
            GameLogic = new SabberStoneGameLogic(Goal, hierarchicalExpansion: true);

            // The side information strategy needs access to several of these.
            SideInformationStrategy = new SabberStoneSideInformationStrategy(Playout, Evaluation, GameLogic);

            // The sampling strategy used to sample actions during the generation phase.
            SamplingStrategy = new SabberStoneLSISamplingStrategy(GameLogic);
        }

        #endregion

        #region Public Methods

        #region ISabberStoneBot

        /// <inheritdoc />
        public SabberStoneAction Act(SabberStoneState state) {
            var timer = Stopwatch.StartNew();
            var stateCopy = (SabberStoneState)state.Copy();

            if (_debug) Console.WriteLine();
            if (_debug) Console.WriteLine(Name());
            if (_debug) Console.WriteLine($"Starting an LSI search in turn {(stateCopy.Game.Turn + 1) / 2}");
            
            // Create a new LSI search
            var search = new LSI<List<SabberStoneAction>, SabberStoneState, SabberStoneAction, object, TreeSearchNode<SabberStoneState, SabberStoneAction>, OddmentTable<SabberStonePlayerTask>>(
                SideInformationStrategy,
                SamplingStrategy,
                Playout,
                Evaluation,
                GameLogic,
                BudgetEstimationStrategy
                );
            
            // Reset the solutions collection
            EnsembleSolutions = new List<SabberStoneAction>();

            // Create a SearchContext that just holds the current state as Source and the Search.
            var context = SearchContext<List<SabberStoneAction>, SabberStoneState, SabberStoneAction, object, SabberStoneAction>.Context(EnsembleSolutions, stateCopy, null, null, search, null);
            // The Playout strategy will call the Goal strategy from the context, so we set it here
            context.Goal = Goal;

            // Execute the search
            Ensemble.EnsembleSearch(context, Searcher.Search, EnsembleSize);
            SamplesSpent = EnsembleSolutions.Sum(i => i.BudgetUsed);

            // Determine a solution
            var solution = Searcher.VoteForSolution(EnsembleSolutions, state);

            timer.Stop();
            if (_debug) Console.WriteLine();
            if (_debug) Console.WriteLine($"LSI returned with solution: {solution}");
            if (_debug) Console.WriteLine($"My total calculation time was: {timer.ElapsedMilliseconds}ms");

            // Check if the solution is a complete action.
            if (!solution.IsComplete()) {
                // Otherwise add an End-Turn task before returning.
                solution.Tasks.Add((SabberStonePlayerTask)EndTurnTask.Any(Player));
            }

            // If we are estimating the budget by using the previous search's results, save these now
            if (BudgetEstimation == BudgetEstimationType.PreviousSearchAverage && BudgetEstimationStrategy is PreviousSearchAverageBudgetEstimationStrategy estimationStrategy) {
                estimationStrategy.PreviousSearchTime = timer.ElapsedMilliseconds;
                estimationStrategy.PreviousSearchIterations = SamplesSpent;
            }

            if (_debug) Console.WriteLine();
            return solution;
        }

        /// <inheritdoc />
        public void SetController(Controller controller) {
            Player = controller;

            MyPlayoutBot.SetController(Player);
            OpponentPlayoutBot.SetController(Player.Opponent);

            // Set the playout bots correctly if we are using PlayoutStrategySabberStone
            Playout.AddPlayoutBot(Player.Id, MyPlayoutBot);
            Playout.AddPlayoutBot(Player.Opponent.Id, OpponentPlayoutBot);

            // Create the searcher that will handle the searching and some administrative tasks
            Searcher = new SabberStoneSearch(Player, _debug);
        }

        /// <inheritdoc />
        public int PlayerID() {
            return Player.Id;
        }

        /// <inheritdoc />
        public string Name() {
            var it = Samples != Constants.DEFAULT_COMPUTATION_ITERATION_BUDGET ? $"_{Samples}it" : "";
            var ti = Time != Constants.DEFAULT_COMPUTATION_TIME_BUDGET ? $"_{Time}ti" : "";
            var ptc = PlayoutTurnCutoff != Constants.DEFAULT_PLAYOUT_TURN_CUTOFF ? $"_{PlayoutTurnCutoff}tc" : "";
            var es = EnsembleSize > 1 ? $"_{EnsembleSize}es" : "";
            var pi = PerfectInformation ? "_PI" : "";
            return $"{BOT_NAME}{it}{ti}{ptc}{es}{pi}";
        }

        /// <inheritdoc />
        public long BudgetSpent() {
            return SamplesSpent;
        }

        /// <inheritdoc />
        public int MaxDepth() {
            return 1;
        }

        #endregion

        #endregion

    }

}
