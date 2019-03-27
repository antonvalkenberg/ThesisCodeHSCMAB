using System;
using System.Collections.Generic;
using System.Linq;
using AVThesis.Datastructures;
using AVThesis.SabberStone.Strategies;
using SabberStoneCore.Enums;
using SabberStoneCore.Model.Entities;

/// <summary>
/// Written by A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by G.J. Roelofs and T. Aliyev.
/// </summary>
namespace AVThesis.SabberStone.Bots {

    /// <summary>
    /// A bot that uses the Move-Average Sampling Technique (MAST) to determine which moves to play.
    /// </summary>
    public class MASTPlayoutBot : ISabberStoneBot {

        #region Enums

        /// <summary>
        /// An enumeration of different types of strategies for selecting actions in MAST.
        /// </summary>
        public enum SelectionType {
            EGreedy, UCB
        }

        #endregion

        #region Constants

        private const string BOT_NAME = "MASTPlayoutBot";
        
        #endregion

        #region Properties

        /// <summary>
        /// How the actions should be selected.
        /// </summary>
        public SelectionType Selection { get; set; }

        /// <summary>
        /// The player this bot represents.
        /// </summary>
        public Controller Player { get; set; }

        /// <summary>
        /// A bot that plays random moves.
        /// </summary>
        public RandomBot RandomPlayoutBot { get; set; }

        /// <summary>
        /// The evaluation strategy used when evaluating the end state of the simulation phase.
        /// </summary>
        public EvaluationStrategyHearthStone Evaluation { get; set; }

        /// <summary>
        /// The playout strategy being used during the simulation phase.
        /// </summary>
        public PlayoutStrategySabberStone Playout { get; set; }

        /// <summary>
        /// The table of data on tasks, indexed by a task's hashcode.
        /// </summary>
        public Dictionary<int, PlayerTaskStatistics> MASTTable { get; set; }

        /// <summary>
        /// The actions taken during playout.
        /// </summary>
        public List<SabberStoneAction> ActionsTaken { get; set; }

        /// <summary>
        /// The setting for when the e-greedy strategy should exploit the best action (i.e. be greedy).
        /// Note: the chance of selecting greedily is 1 minus this threshold.
        /// </summary>
        public double EGreedyThreshold { get; set; }

        /// <summary>
        /// The value for the `C' constant in the UCB1 formula.
        /// </summary>
        public double UCBConstantC { get; set; }

        #endregion

        #region Constuctors

        /// <summary>
        /// Creates a new instance of the MASTPlayoutBot.
        /// </summary>
        /// <param name="selection">The type of selection to use.</param>
        /// <param name="evaluation">The EvaluationStrategy used for evaluating a SabberStoneState.</param>
        /// <param name="playout">The PlayoutStrategy used for simulating a game.</param>
        /// <param name="eGreedyThreshold">[Optional] Threshold for e-greedy selection. Default value is <see cref="Constants.DEFAULT_E_GREEDY_THRESHOLD"/>.</param>
        /// <param name="ucbConstantC">[Optional] Value for the c-constant in the UCB1 formula. Default value is <see cref="Constants.DEFAULT_UCB1_C"/>.</param>
        public MASTPlayoutBot(SelectionType selection, EvaluationStrategyHearthStone evaluation, PlayoutStrategySabberStone playout, double eGreedyThreshold = Constants.DEFAULT_E_GREEDY_THRESHOLD, double ucbConstantC = Constants.DEFAULT_UCB1_C) {
            Selection = selection;
            RandomPlayoutBot = new RandomBot(filterDuplicatePositionTasks: true);
            Evaluation = evaluation;
            Playout = playout;
            EGreedyThreshold = eGreedyThreshold;
            UCBConstantC = ucbConstantC;

            playout.SimulationCompleted += SimulationCompleted;

            MASTTable = new Dictionary<int, PlayerTaskStatistics>();
            ActionsTaken = new List<SabberStoneAction>();
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Reset the data stored in the MAST-table.
        /// </summary>
        private void ResetMASTData() {
            MASTTable = new Dictionary<int, PlayerTaskStatistics>();
            ActionsTaken = new List<SabberStoneAction>();
        }

        /// <summary>
        /// Add data for an action to this bot's MAST-table.
        /// </summary>
        /// <param name="action">The action to add the data for.</param>
        /// <param name="value">The value to add to the action.</param>
        private void AddData(SabberStoneAction action, double value) {
            // Adjust the values for all tasks in the action
            foreach (var task in action.Tasks) {
                var hashKey = task.GetHashCode();
                if (!MASTTable.ContainsKey(hashKey)) MASTTable.Add(hashKey, new PlayerTaskStatistics(task, value));
                else MASTTable[hashKey].AddValue(value);
            }
        }

        /// <summary>
        /// Selects an action using the e-greedy algorithm.
        /// </summary>
        /// <param name="state">The game state.</param>
        /// <returns><see cref="SabberStoneAction"/>.</returns>
        private SabberStoneAction SelectEGreedy(SabberStoneState state) {
            // Determine whether or not to be greedy (chance is 1-e to use best action)
            if (Util.RNG.NextDouble() < EGreedyThreshold)
                // Explore a random action
                return RandomPlayoutBot.CreateRandomAction(state);

            var action = new SabberStoneAction();
            var stateClone = state.Game.Clone();
            // Repeatedly exploit the highest (average) reward task that is available in this state
            do {
                SabberStonePlayerTask selectedTask;
                // Get the stats of the tasks currently available in this state
                var availableTasks = stateClone.Game.CurrentPlayer.Options().Where(i => i.ZonePosition <= 0).Select(i => (SabberStonePlayerTask)i).ToList();
                var availableTaskHashes = availableTasks.Select(i => i.GetHashCode()).ToList();
                var availableStatistics = MASTTable.Where(i => availableTaskHashes.Contains(i.Key)).ToList();

                // Find the task with the highest average value
                var bestTask = availableStatistics.OrderByDescending(i => i.Value.AverageValue()).FirstOrDefault();

                // If no best task was found, randomly choose an available task
                if (bestTask.IsDefault()) {
                    var randomTask = availableTasks.RandomElementOrDefault();
                    // If we also can't randomly find a task, stop
                    if (randomTask == null) break;
                    selectedTask = randomTask;
                }
                else {
                    // Find all available tasks that have an average value similar to the best
                    var bestValue = bestTask.Value.AverageValue();
                    var compTasks = availableStatistics.Where(i =>
                        Math.Abs(i.Value.AverageValue() - bestValue) < AVThesis.Constants.DOUBLE_EQUALITY_TOLERANCE).ToList();
                    // Select one of the tasks
                    selectedTask = compTasks.RandomElementOrDefault().Value.Task;
                }

                // Add the task to the action we are building
                action.AddTask(selectedTask);
                // Process the task
                stateClone.Process(selectedTask.Task);

            // Continue until we have created a complete action, or the game has completed
            } while (!action.IsComplete() && stateClone.Game.State != State.COMPLETE);

            // Return the action we've created
            return action;
        }

        /// <summary>
        /// Selects and action using the UCB1 algorithm.
        /// </summary>
        /// <param name="state">The game state.</param>
        /// <returns><see cref="SabberStoneAction"/>.</returns>
        private SabberStoneAction SelectUCB(SabberStoneState state) {
            var action = new SabberStoneAction();
            var stateClone = state.Game.Clone();
            // Repeatedly exploit the highest UCB-value task that is available in this state
            do {
                SabberStonePlayerTask selectedTask;
                // Get the stats of the tasks currently available in this state
                var availableTasks = stateClone.Game.CurrentPlayer.Options().Where(i => i.ZonePosition <= 0).Select(i => (SabberStonePlayerTask)i).ToList();
                var availableTaskHashes = availableTasks.Select(i => i.GetHashCode()).ToList();
                var availableStatistics = MASTTable.Where(i => availableTaskHashes.Contains(i.Key)).ToList();
                var totalVisits = availableStatistics.Sum(i => i.Value.Visits);

                // Find the task with the highest UCB value
                var bestTask = availableStatistics.OrderByDescending(i => i.Value.UCB(totalVisits, UCBConstantC)).FirstOrDefault();

                // If no best task was found, randomly choose an available task
                if (bestTask.IsDefault()) {
                    var randomTask = availableTasks.RandomElementOrDefault();
                    // If we also can't randomly find a task, stop
                    if (randomTask == null) break;
                    selectedTask = randomTask;
                }
                else {
                    // Find all available tasks that have an UCB value similar to the best
                    var bestValue = bestTask.Value.UCB(totalVisits, UCBConstantC);
                    var compTasks = availableStatistics.Where(i =>
                        Math.Abs(i.Value.UCB(totalVisits, UCBConstantC) - bestValue) < AVThesis.Constants.DOUBLE_EQUALITY_TOLERANCE).ToList();
                    // Select one of the tasks
                    selectedTask = compTasks.RandomElementOrDefault().Value.Task;
                }

                // Add the task to the action we are building
                action.AddTask(selectedTask);
                // Process the task
                stateClone.Process(selectedTask.Task);

                // Continue until we have created a complete action, or the game has completed
            } while (!action.IsComplete() && stateClone.Game.State != State.COMPLETE);

            // Return the action we've created
            return action;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Handles the <see cref="PlayoutStrategySabberStone.SimulationCompleted"/> event.
        /// </summary>
        /// <param name="sender">The object that triggered the event.</param>
        /// <param name="eventArgs">The arguments of the event.</param>
        public void SimulationCompleted(object sender, PlayoutStrategySabberStone.SimulationCompletedEventArgs eventArgs) {
            // Evaluate the state.
            var value = Evaluation.Evaluate(eventArgs.Context, null, eventArgs.EndState);
            // Add data for all the actions that have been taken.
            foreach (var action in ActionsTaken) {
                AddData(action, value);
            }
            // Clear the taken actions.
            ActionsTaken = new List<SabberStoneAction>();
        }

        /// <summary>
        /// Returns a SabberStoneAction for the current state.
        /// </summary>
        /// <param name="state">The current game state.</param>
        /// <returns>SabberStoneAction</returns>
        public SabberStoneAction Act(SabberStoneState state) {
            // Check to make sure the player to act in the game-state matches our player.
            if (state.CurrentPlayer() != Player.Id) {
                return null;
            }

            SabberStoneAction selectedAction;
            // When we have to act, check which policy we are going to use
            switch (Selection) {
                case SelectionType.UCB:
                    selectedAction = SelectUCB(state);
                    break;
                case SelectionType.EGreedy:
                    selectedAction = SelectEGreedy(state);
                    break;
                default:
                    selectedAction = RandomPlayoutBot.CreateRandomAction(state);
                    break;
            }

            // Remember the action that was selected.
            ActionsTaken.Add(selectedAction);
            return selectedAction;
        }

        /// <summary>
        /// Sets the Controller that the bot represents within a SabberStone Game.
        /// </summary>
        /// <param name="controller">This bot's Controller.</param>
        public void SetController(Controller controller) {
            Player = controller;
            RandomPlayoutBot = new RandomBot(controller, filterDuplicatePositionTasks: true);
            // Also reset the table of statistics
            ResetMASTData();
        }

        /// <summary>
        /// Returns the player's ID.
        /// </summary>
        /// <returns>Integer representing the player's ID.</returns>
        public int PlayerID() => Player.Id;

        /// <summary>
        /// Returns the Bot's name.
        /// </summary>
        /// <returns>String representing the Bot's name.</returns>
        public string Name() {
            string setting;
            switch (Selection) {
                case SelectionType.EGreedy:
                    setting = $"{EGreedyThreshold:F1}";
                    break;
                case SelectionType.UCB:
                    setting = $"{UCBConstantC:F1}";
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            return $"{BOT_NAME}_{Selection}_{setting}";
        }

        /// <inheritdoc />
        public long BudgetSpent() {
            return 0;
        }

        /// <inheritdoc />
        public int MaxDepth() {
            return 0;
        }

        #endregion

    }
}
