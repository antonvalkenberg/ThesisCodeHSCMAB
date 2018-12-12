using System;
using System.Collections.Generic;
using System.Linq;
using AVThesis.Datastructures;
using AVThesis.Game;
using AVThesis.SabberStone;
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
namespace AVThesis.Bots {

    /// <summary>
    /// A bot that plays Hearthstone using Linear Side Information to find its best move.
    /// </summary>
    public class LSIBot : ISabberStoneBot {

        #region Helper Class

        private class ActionValue {
            public SabberStoneAction Action { get; set; }
            public double Value { get; set; }
            public ActionValue(SabberStoneAction action, double value) {
                Action = action;
                Value = value;
            }
        }

        #endregion

        #region Inner Classes

        private class SabberStoneSideInformationStrategy : ISideInformationStrategy<object, SabberStoneState, SabberStoneAction, object, SabberStoneAction, OddmentTable<PlayerTask>> {
            
            #region Properties

            /// <summary>
            /// The bot that is used during the playouts.
            /// </summary>
            private RandomBot PlayoutBot { get; set; }
            
            /// <summary>
            /// The strategy used to play out a game in simulation.
            /// </summary>
            private IPlayoutStrategy<object, SabberStoneState, SabberStoneAction, object, SabberStoneAction> Playout { get; set; }

            /// <summary>
            /// The evaluation strategy for determining the value of samples.
            /// </summary>
            private IStateEvaluation<object, SabberStoneState, SabberStoneAction, object, SabberStoneAction, TreeSearchNode<SabberStoneState, SabberStoneAction>> Evaluation { get; set; }

            /// <summary>
            /// The game specific logic required for searching through SabberStoneStates and SabberStoneActions.
            /// </summary>
            private IGameLogic<object, SabberStoneState, SabberStoneAction, object, SabberStoneAction, SabberStoneAction> GameLogic { get; set; }

            #endregion

            #region Constructor

            /// <summary>
            /// Creates a new instance.
            /// </summary>
            /// <param name="playoutBot">The bot that is used during the playouts.</param>
            /// <param name="playout">The strategy used to play out a game in simulation.</param>
            /// <param name="evaluation">The evaluation strategy for determining the value of samples.</param>
            /// <param name="gameLogic">The game specific logic required for searching through SabberStoneStates and SabberStoneActions.</param>
            public SabberStoneSideInformationStrategy(RandomBot playoutBot, IPlayoutStrategy<object, SabberStoneState, SabberStoneAction, object, SabberStoneAction> playout, IStateEvaluation<object, SabberStoneState, SabberStoneAction, object, SabberStoneAction, TreeSearchNode<SabberStoneState, SabberStoneAction>> evaluation, IGameLogic<object, SabberStoneState, SabberStoneAction, object, SabberStoneAction, SabberStoneAction> gameLogic) {
                PlayoutBot = playoutBot;
                Playout = playout;
                Evaluation = evaluation;
                GameLogic = gameLogic;
            }

            #endregion

            #region Public Methods

            /// <inheritdoc />
            public OddmentTable<PlayerTask> Create(SearchContext<object, SabberStoneState, SabberStoneAction, object, SabberStoneAction> context, int samplesForGeneration) {

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

                var table = new Dictionary<PlayerTask, double>();

                // So, we have a number of samples to use
                // For each of those, generate a random SabberStoneAction and playout
                for (int i = 0; i < samplesForGeneration; i++) {

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
                var oddmentTable = new OddmentTable<PlayerTask>();
                foreach (var kvPair in table) {
                    oddmentTable.Add(kvPair.Key, kvPair.Value, recalculate: false);
                }

                oddmentTable.Recalculate();

                return oddmentTable;
            }

            #endregion

        }

        private class SabberStoneLSISamplingStrategy : ILSISamplingStrategy<SabberStoneState, SabberStoneAction, OddmentTable<PlayerTask>> {
            
            #region Properties

            /// <summary>
            /// The game specific logic required for searching through SabberStoneStates and SabberStoneActions.
            /// </summary>
            private IGameLogic<object, SabberStoneState, SabberStoneAction, object, SabberStoneAction, SabberStoneAction> GameLogic { get; set; }

            #endregion

            #region Constructor

            /// <summary>
            /// Constructs a new instance.
            /// </summary>
            /// <param name="gameLogic">The game specific logic required for searching through SabberStoneStates and SabberStoneActions.</param>
            public SabberStoneLSISamplingStrategy(IGameLogic<object, SabberStoneState, SabberStoneAction, object, SabberStoneAction, SabberStoneAction> gameLogic) {
                GameLogic = gameLogic;
            }

            #endregion

            #region Private Methods

            /// <summary>
            /// Expands a specific state and returns all available PlayerTasks.
            /// Note: it is assumed that the expansion is performed Hierarchically and therefore only contains single PlayerTask SabberStoneActions.
            /// </summary>
            /// <param name="state">The game state.</param>
            /// <returns>Collection of PlayerTasks that are available in the provided state.</returns>
            private List<PlayerTask> GetAvailablePlayerTasks(SabberStoneState state) {
                var availableTasks = new List<PlayerTask>();
                foreach (var expandedAction in GameLogic.Expand(null, state)) {
                    availableTasks.Add(expandedAction.Tasks.First());
                }
                return availableTasks;
            }

            #endregion
            
            #region Public Methods

            /// <inheritdoc />
            public SabberStoneAction Sample(SabberStoneState state, OddmentTable<PlayerTask> sideInformation) {

                var action = new SabberStoneAction();
                while (!action.IsComplete()) {
                    // Sample a task from the OddmentTable
                    var task = sideInformation.Next();
                    // Check if the task is available in the current state
                    if (GetAvailablePlayerTasks(state).Contains(task)) {
                        action.AddTask(task);
                        state.Game.Process(task);
                    }
                }

                return action;
            }

            /// <inheritdoc />
            public SabberStoneAction Sample(SabberStoneState state) {
                return SabberStoneAction.CreateNullMove(state.CurrentPlayer() == state.Player1.Id ? state.Player1 : state.Player2);
            }

            #endregion
            
        }

        #endregion

        #region Constants

        private const int LSI_SAMPLES_FOR_GENERATION = 250;
        private const int LSI_SAMPLES_FOR_EVALUATION = 750;
        private const int PLAYOUT_TURN_CUTOFF = 2;
        private const string _botName = "LSIBot";

        #endregion

        #region Properties

        /// <summary>
        /// The player this bot is representing in a game of SabberStone.
        /// </summary>
        public Controller Player { get; set; }

        /// <summary>
        /// The bot that is used during the playouts.
        /// </summary>
        public RandomBot PlayoutBot { get; set; }

        /// <summary>
        /// The strategy used to determine if a playout has reached its goal state.
        /// </summary>
        public IGoalStrategy<object, SabberStoneState, SabberStoneAction, object, SabberStoneAction> Goal { get; set; }

        /// <summary>
        /// The strategy used to play out a game in simulation.
        /// </summary>
        public IPlayoutStrategy<object, SabberStoneState, SabberStoneAction, object, SabberStoneAction> Playout { get; set; }

        /// <summary>
        /// The evaluation strategy for determining the value of samples.
        /// </summary>
        public IStateEvaluation<object, SabberStoneState, SabberStoneAction, object, SabberStoneAction, TreeSearchNode<SabberStoneState, SabberStoneAction>> Evaluation { get; set; }

        /// <summary>
        /// The game specific logic required for searching through SabberStoneStates and SabberStoneActions
        /// </summary>
        public IGameLogic<object, SabberStoneState, SabberStoneAction, object, SabberStoneAction, SabberStoneAction> GameLogic { get; set; }
        
        /// <summary>
        /// The strategy for creating the side information.
        /// </summary>
        public ISideInformationStrategy<object, SabberStoneState, SabberStoneAction, object, SabberStoneAction, OddmentTable<PlayerTask>> SideInformationStrategy { get; set; }

        /// <summary>
        /// The strategy for sampling actions during the generation phase of LSI.
        /// </summary>
        public ILSISamplingStrategy<SabberStoneState, SabberStoneAction, OddmentTable<PlayerTask>> SamplingStrategy { get; set; }

        public int SamplesUsedGeneration { get; set; }
        public int SamplesUsedEvaluation { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructs a new instance of LSIBot with a <see cref="Controller"/> representing the player.
        /// </summary>
        /// <param name="player">The player.</param>
        public LSIBot(Controller player) {
            Player = player;

            // Set the playout bot
            PlayoutBot = new RandomBot();

            // LSI will need a goal-strategy to determine when a simulation is done
            Goal = new GoalStrategyTurnCutoff(PLAYOUT_TURN_CUTOFF);

            // LSI will need a playout-strategy to run the simulations
            Playout = new PlayoutStrategySabberStone(PlayoutBot);

            // LSI will need an evaluation-strategy to evaluate the strength of samples
            Evaluation = new EvaluationStrategyHearthStone();

            // Application will be handled by the GameLogic
            // Hierarchical Expansion is set to TRUE because of incremental task validation during action sampling.
            GameLogic = new SabberStoneGameLogic(true, Goal);

            // The side information strategy needs access to several of these.
            SideInformationStrategy = new SabberStoneSideInformationStrategy(PlayoutBot, Playout, Evaluation, GameLogic);

            // The sampling strategy used to sample actions during the generation phase.
            SamplingStrategy = new SabberStoneLSISamplingStrategy(GameLogic);
        }

        #endregion

        #region Public Methods

        #region ISabberStoneBot

        /// <inheritdoc />
        public SabberStoneAction Act(SabberStoneState state) {
            var timer = System.Diagnostics.Stopwatch.StartNew();
            var stateCopy = (SabberStoneState)state.Copy();

            // Let's keep track of how many samples LSI actually uses.
            SamplesUsedEvaluation = 0;
            SamplesUsedGeneration = 0;
            // Adjust the allowed budget for evaluation, because LSI will use more.
            // This factor is pre-set and empirically determined.
            int adjustedSamplesForEvaluation = (int)(LSI_SAMPLES_FOR_EVALUATION * 1.0);

            // Create a new LSI search
            var search = new LSI<object, SabberStoneState, SabberStoneAction, object, TreeSearchNode<SabberStoneState, SabberStoneAction>, OddmentTable<PlayerTask>>(
                LSI_SAMPLES_FOR_GENERATION,
                adjustedSamplesForEvaluation,
                SideInformationStrategy,
                SamplingStrategy,
                Playout,
                Evaluation,
                GameLogic);

            // Create a SearchContext that just holds the current state as Source and the Search.
            var context = SearchContext<object, SabberStoneState, SabberStoneAction, object, SabberStoneAction>.Context(null, stateCopy,
                null, null, search, null);
            // The Playout strategy will call the Goal strategy from the context, so we set it here
            context.Goal = Goal;

            Console.WriteLine();
            Console.WriteLine(Name());
            Console.WriteLine("Starting an LSI-search in turn " + (stateCopy.Game.Turn + 1) / 2);

            // Execute the search
            context.Execute();

            var solution = context.Solution;
            var time = timer.ElapsedMilliseconds;
            Console.WriteLine($"LSI returned with solution: {solution}");
            Console.WriteLine($"My action calculation time was: {time} ms.");
            Console.WriteLine();

            // Check if the solution is a complete action.
            if (solution.IsComplete()) return solution;
            // Otherwise add an End-Turn task before returning.
            Console.WriteLine("Solution was an incomplete action; adding End-Turn task.");
            solution.Tasks.Add(EndTurnTask.Any(Player));
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
            return _botName;
        }

        #endregion

        #endregion

    }
}
