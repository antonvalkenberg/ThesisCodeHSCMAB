using System;
using System.Collections.Generic;
using System.Linq;
using AVThesis.Datastructures;
using AVThesis.SabberStone.Strategies;
using SabberStoneCore.Model.Entities;
using SabberStoneCore.Tasks;

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

        #region Fields

        private const double EGREEDY_E = 0.2;
        private const string BOT_NAME = "MASTPlayoutBot";
        private readonly Random _rng = new Random();

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

        #endregion

        #region Constuctors

        /// <summary>
        /// Creates a new instance of the MASTPlayoutBot.
        /// </summary>
        /// <param name="selection">The type of selection to use.</param>
        /// <param name="evaluation">The EvaluationStrategy used for evaluating a SabberStoneState.</param>
        /// <param name="playout">The PlayoutStrategy used for simulating a game.</param>
        public MASTPlayoutBot(SelectionType selection, EvaluationStrategyHearthStone evaluation, PlayoutStrategySabberStone playout) {
            Selection = selection;
            RandomPlayoutBot = new RandomBot();
            Evaluation = evaluation;
            Playout = playout;
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
            if (_rng.NextDouble() < EGREEDY_E)
                // Explore a random action
                return RandomPlayoutBot.CreateRandomAction(state);

            var action = new SabberStoneAction();
            var stateClone = state.Game.Clone();
            var currentPlayerId = stateClone.CurrentPlayer.Id;
            SabberStonePlayerTask selectedTask;
            // Repeatedly exploit the highest (average) reward task that is available in this state
            do {
                // Get the stats of the tasks currently available in this state
                var availableTasks = stateClone.Game.CurrentPlayer.Options().Select(i => (SabberStonePlayerTask)i).ToList();
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
                    var compTasks = availableStatistics.Where(i =>
                        Math.Abs(i.Value.AverageValue() - bestTask.Value.AverageValue()) <
                        Constants.DOUBLE_EQUALITY_TOLERANCE).ToList();
                    // Select one of the tasks
                    selectedTask = compTasks.Count() > 1 ? compTasks.RandomElementOrDefault().Value.Task : bestTask.Value.Task;
                }

                // Add the task to the action we are building
                action.AddTask(selectedTask);
                // Process the task
                stateClone.Process(selectedTask.Task);

            // Continue while it is still our turn and we haven't yet selected to end the turn.
            } while (stateClone.CurrentPlayer.Id == currentPlayerId && selectedTask.Task.PlayerTaskType != PlayerTaskType.END_TURN);

            // Return the action we've created
            return action;
        }

        /// <summary>
        /// Selects and action using the UCB1 algorithm.
        /// </summary>
        /// <param name="state">The game state.</param>
        /// <returns><see cref="SabberStoneAction"/>.</returns>
        private SabberStoneAction SelectUCB(SabberStoneState state) {
            //TODO Implement UCB selection for MAST
            throw new NotImplementedException();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Handles the <see cref="PlayoutStrategySabberStone.SimulationCompleted"/> event.
        /// </summary>
        /// <param name="sender">The object that triggered the event.</param>
        /// <param name="eventArgs">The arguments of the event.</param>
        public void SimulationCompleted(object sender, PlayoutStrategySabberStone.SimulationCompletedEventArgs eventArgs) {
            //TODO check to make sure we are evaluating the board from the correct viewpoint
            
            // Evaluate the state.
            var eval = Evaluation.Evaluate(eventArgs.Context, null, eventArgs.EndState);
            // Add data for all the actions that have been taken.
            foreach (var action in ActionsTaken) {
                AddData(action, eval);
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
            // Check to make sure the player to act in the gamestate matches our player.
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
            RandomPlayoutBot = new RandomBot(controller);
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
            return $"{BOT_NAME}_{Selection}";
        }

        #endregion

    }
}
