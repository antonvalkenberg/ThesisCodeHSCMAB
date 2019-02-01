using System;
using System.Collections.Generic;
using System.Linq;
using AVThesis.Datastructures;
using AVThesis.SabberStone.Strategies;
using AVThesis.Search;
using AVThesis.Search.Tree.MCTS;
using SabberStoneCore.Model.Entities;
using SabberStoneCore.Tasks;
using SabberStoneCore.Tasks.PlayerTasks;

/// <summary>
/// Written by A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by G.J. Roelofs and T. Aliyev.
/// </summary>
namespace AVThesis.SabberStone {

    /// <summary>
    /// Represents a single search in SabberStone and fulfills some administrative tasks.
    /// </summary>
    public class SabberStoneSearch {

        #region Fields

        private readonly bool _debug;

        #endregion

        #region Properties

        /// <summary>
        /// The player in a game of SabberStone that this search is for.
        /// </summary>
        public Controller Player { get; set; }

        /// <summary>
        /// Statistics on PlayerTasks indexed by the task's hashcode.
        /// </summary>
        public Dictionary<int, PlayerTaskStatistics> TaskStatistics { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of this searcher.
        /// </summary>
        /// <param name="player">The player in SabberStone to search for.</param>
        /// <param name="debugToConsole">[Optional] Whether or not to write debug information to the console. Default value is false.</param>
        public SabberStoneSearch(Controller player, bool debugToConsole = false) {
            Player = player;
            _debug = debugToConsole;
            TaskStatistics = new Dictionary<int, PlayerTaskStatistics>();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Runs a single search.
        /// Note: this method is called by the <see cref="EnsembleStrategySabberStone"/>.
        /// </summary>
        /// <param name="context">The context of the search.</param>
        /// <param name="state">The current game state.</param>
        /// <returns>SabberStoneAction that is the solution to the search.</returns>
        public SabberStoneAction Search(SearchContext<List<SabberStoneAction>, SabberStoneState, SabberStoneAction, object, SabberStoneAction> context, SabberStoneState state) {
            var timer = System.Diagnostics.Stopwatch.StartNew();
            if (_debug) Console.WriteLine();

            // Execute the search
            context.Execute();

            // Check if the search was successful
            if (context.Status != SearchContext<List<SabberStoneAction>, SabberStoneState, SabberStoneAction, object, SabberStoneAction>.SearchStatus.Success) {
                // TODO in case of search failure: throw exception, or print error.
                return SabberStoneAction.CreateNullMove(state.Game.CurrentPlayer);
            }

            var solution = context.Solution;
            // Retrieve the task values from the solution strategy and process them into the property
            var solutionStrategy = (SolutionStrategySabberStone)((MCTS<List<SabberStoneAction>, SabberStoneState, SabberStoneAction, object, SabberStoneAction>)context.Search).SolutionStrategy;
            foreach (var tuple in solutionStrategy.TaskValues) {
                var taskHash = tuple.Item1.GetHashCode();
                if (!TaskStatistics.ContainsKey(taskHash)) TaskStatistics.Add(taskHash, new PlayerTaskStatistics(tuple.Item1, tuple.Item2));
                else TaskStatistics[taskHash].AddValue(tuple.Item2);
            }
            // Make sure to clear the values for the next search
            solutionStrategy.ClearTaskValues();

            var time = timer.ElapsedMilliseconds;
            if (_debug) Console.WriteLine($"Searcher returned with solution: {solution}");
            if (_debug) Console.WriteLine($"Calculation time was: {time} ms.");

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
        /// Determines the best tasks for the game state based on the provided statistics and creates a <see cref="SabberStoneAction"/> from them.
        /// </summary>
        /// <param name="state">The game state to create the best action for.</param>
        /// <returns><see cref="SabberStoneAction"/> created from the best individual tasks available in the provided state.</returns>
        public SabberStoneAction DetermineBestTasks(SabberStoneState state) {
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
                var availableTasks = clonedPlayer.Options().Select(i => ((SabberStonePlayerTask)i).GetHashCode());
                bestTask = TaskStatistics.Where(i => availableTasks.Contains(i.Key)).OrderByDescending(i => i.Value.AverageValue()).FirstOrDefault();

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

        /// <summary>
        /// Resets the statistics on the individual tasks.
        /// </summary>
        public void ResetTaskStatistics() {
            TaskStatistics = new Dictionary<int, PlayerTaskStatistics>();
        }

        #endregion
    }

}
