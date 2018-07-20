using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using AVThesis.Datastructures;
using AVThesis.Game;
using AVThesis.SabberStone;
using AVThesis.SabberStone.Strategies;
using AVThesis.Search;
using AVThesis.Search.Tree;
using SabberStoneCore.Model.Entities;
using SabberStoneCore.Tasks;

/// <summary>
/// Written by A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by G.J. Roelofs and T. Aliyev.
/// </summary>
namespace AVThesis.Bots {

    /// <inheritdoc />
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

        #region Constants

        private const int LSI_SAMPLES_FOR_GENERATION = 250;
        private const int LSI_SAMPLES_FOR_EVALUATION = 750;
        private const int PLAYOUT_TURN_CUTOFF = 2;

        #endregion

        #region Fields

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
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Generates the interesting subset of actions C* from C.
        /// 
        /// 1) Generate a weight function R^ from PartialActions(adopting the linear side information assumption).
        /// 2) Schematically generating a probability distribution D_R^ over CombinedAction space C, biased "towards" R^.
        /// 3) Sample a number of CombinedActions C* from D_R^.
        /// </summary>
        /// <param name="context">The current search context.</param>
        /// <param name="samplesForGeneration">The number of samples allowed during the generation phase.</param>
        /// <param name="samplesForEvaluation">The number of samples allowed during the evaluation phase.</param>
        /// <returns></returns>
        private List<SabberStoneAction> Generate(SearchContext<object, SabberStoneState, SabberStoneAction, object, SabberStoneAction> context, int samplesForGeneration, int samplesForEvaluation) {

            // Create the Side Information using the allowed number of generation samples.
            var sideInfo = SideInformation(context, samplesForGeneration);

            // Create combined-actions by sampling the side information.
            List<SabberStoneAction> sampledActions = new List<SabberStoneAction>();
            for (int i = 0; i < samplesForEvaluation; i++) {
                sampledActions.Add(SampleAction(context, sideInfo));
            }

            return sampledActions;
        }

        /// <summary>
        /// Creates an action for the specified context by sampling the provided OddmentTable.
        /// </summary>
        /// <param name="context">The game state to create an action for.</param>
        /// <param name="taskWeights">The OddmentTable to sample from.</param>
        /// <returns>SabberStoneAction created by sampling the provided OddmentTable.</returns>
        private SabberStoneAction SampleAction(SearchContext<object, SabberStoneState, SabberStoneAction, object, SabberStoneAction> context, OddmentTable<PlayerTask> taskWeights) {

            var currentState = (SabberStoneState)context.Source.Copy();
            var action = new SabberStoneAction();
            while (!action.IsComplete()) {
                // Sample a task from the OddmentTable
                var task = taskWeights.Next();
                // Check if the task is available in the current state
                if (GetAvailablePlayerTasks(currentState).Contains(task)) {
                    action.AddTask(task);
                    currentState.Game.Process(task);
                }
            }

            return action;
        }

        /// <summary>
        /// Produces the side info, a list of distributions for individual actions in dimensions to an average score.
        /// </summary>
        /// <param name="context">The current search context.</param>
        /// <param name="samplesForGeneration">The number of samples allowed during the generation phase.</param>
        /// <returns></returns>
        private OddmentTable<PlayerTask> SideInformation(SearchContext<object, SabberStoneState, SabberStoneAction, object, SabberStoneAction> context, int samplesForGeneration) {

            // So we have an issue here, because it's Hearthstone we don't know in advance how many dimensions we have.
            //      -> We can just evenly distribute budget over the currently available dimensions
            //      -- Reason is that some dimensions would be `locked' until after a specific dimensions is explored
            //      -> Another option is to expand all possible sequences, this might be too complex though...

            // In Hearthstone we don't really have multiple available actions per dimension
            // It's more like you either play/attack or not
            // Although each minion can have multiple choices on what to attack

            // We can still try to distilate individual actions
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

        /// <summary>
        /// Select the best combined-action from C*.
        /// </summary>
        /// <param name="context">The current search context.</param>
        /// <param name="samplesForEvaluation">The number of samples allowed during the evaluation phase.</param>
        /// <param name="generatedActions">C*, the subset of combined-actions generated in the generation phase.</param>
        /// <returns></returns>
        private SabberStoneAction Evaluate(SearchContext<object, SabberStoneState, SabberStoneAction, object, SabberStoneAction> context, int samplesForEvaluation, List<SabberStoneAction> generatedActions) {

            // Create some way to track the value of each combined action
            var actionValues = new List<ActionValue>();
            foreach (var action in generatedActions) {
                actionValues.Add(new ActionValue(action, 0));
            }

            // Determine the exact number of iterations of the halving routine we need to do to reduce the generated-actions to a singular item
            // For example : Ceiling(Log_2(100))=7 -> 100-50-25-13-7-4-2-1
            var timesToHalf = Math.Max(1, (int)Math.Ceiling(Math.Log(generatedActions.Count, 2)));

            for (int i = 0; i < timesToHalf; i++) {
                // Determine the number of samples that can be used per action during this iteration
                // For example, when we have 1000 samples to divide over 100 generated-actions,
                // the level of samples-per-action work out to be: 1-2-5-10-20-35-71, for a total of 877 samples
                // This also illustrates the need for Floor instead of Round or Ceiling
                var samplesPerAction = Math.Max(1, (int)Math.Floor(samplesForEvaluation / (actionValues.Count * timesToHalf * 1.0)));

                actionValues = SelectBestHalf(context, actionValues, samplesPerAction);
            }

            // Return the resulting best action.
            if (actionValues.IsNullOrEmpty()) return null;
            return actionValues.First().Action;
        }

        /// <summary>
        /// Expands a specifc state and returns all available PlayerTasks.
        /// Note: it is assumed that the expansion is performed Hierachically and therefore only contains single PlayerTask SabberStoneActions.
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

        /// <summary>
        /// Selects the best half of a collection of actions by evaluating them based on the provided search context.
        /// </summary>
        /// <param name="context">The current search context.</param>
        /// <param name="actions">The collection of actions to filter.</param>
        /// <param name="samplesPerAction">How many samples to run per action.</param>
        /// <returns>Collection of actions which Count is half of the original collection, rounded up. This collection is ordered by descending value.</returns>
        private List<ActionValue> SelectBestHalf(SearchContext<object, SabberStoneState, SabberStoneAction, object, SabberStoneAction> context, List<ActionValue> actions, int samplesPerAction) {

            foreach (var item in actions) {

                var newState = GameLogic.Apply(context, (SabberStoneState)context.Source.Copy(), item.Action);

                double value = 0;
                for (int i = 0; i < samplesPerAction; i++) {
                    value += Evaluation.Evaluate(context, null, Playout.Playout(context, newState));
                }
                item.Value = value;
            }

            var half = (int)Math.Max(1, Math.Ceiling(actions.Count / 2.0));

            return actions.OrderByDescending(i => i.Value).Take(half).ToList();
        }

        #endregion

        #region Public Methods

        #region ISabberStoneBot

        /// <inheritdoc />
        public SabberStoneAction Act(SabberStoneState state) {

            // Let's keep track of how many samples LSI actually uses.
            SamplesUsedEvaluation = 0;
            SamplesUsedGeneration = 0;
            // Adjust the allowed budget for evaluation, because LSI will use more.
            // This factor is pre-set and empirically determined.
            int adjustedSamplesForEvaluation = (int)(LSI_SAMPLES_FOR_EVALUATION * 1.0);


            // Create a SearchContext that just holds the current state as Source
            var context = SearchContext<object, SabberStoneState, SabberStoneAction, object, SabberStoneAction>.Context(null, state,
                null, null, null, null);
            // The Playout strategy will call the Goal strategy from the context, so we set it here
            context.Goal = Goal;

            // LSI divides the search process into two separate phases

            // Generate a subset (C*) from all possible combined-actions (C)
            var cStar = Generate(context, LSI_SAMPLES_FOR_GENERATION, adjustedSamplesForEvaluation);

            // Evaluate and return the best combined-action in C*
            return Evaluate(context, adjustedSamplesForEvaluation, cStar);
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
