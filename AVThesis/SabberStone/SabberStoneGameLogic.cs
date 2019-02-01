using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AVThesis.Datastructures;
using AVThesis.Game;
using AVThesis.Search;
using SabberStoneCore.Tasks.PlayerTasks;
using State = SabberStoneCore.Enums.State;

/// <summary>
/// Written by A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by G.J. Roelofs and T. Aliyev.
/// </summary>
namespace AVThesis.SabberStone {

    /// <summary>
    /// Handles the game specific logic required for search in SabberStone.
    /// </summary>
    public class SabberStoneGameLogic : IGameLogic<List<SabberStoneAction>, SabberStoneState, SabberStoneAction, object, SabberStoneAction, SabberStoneAction> {
        
        #region SabberStoneMoveGenerator

        /// <summary>
        /// Enumerates a collection of SabberStoneActions.
        /// </summary>
        public sealed class SabberStoneMoveGenerator : IPositionGenerator<SabberStoneAction> {

            #region Constants

            private const int STARTING_POSITION = -1;

            #endregion

            #region Fields

            private int _position = STARTING_POSITION;
            private List<SabberStoneAction> _list;

            #endregion

            #region Properties

            /// <summary>
            /// The current position of the enumerator in the collection.
            /// </summary>
            public int Position { get => _position; set => _position = value; }

            #endregion

            #region Constructor

            /// <summary>
            /// Constructs a new instance to enumerate the provided collection.
            /// </summary>
            /// <param name="actions">The collection of actions to enumerate.</param>
            public SabberStoneMoveGenerator(List<SabberStoneAction> actions) {
                _list = actions;
            }

            #endregion

            #region IPositionGenerator

            /// <summary>
            /// Whether or not this enumerator can proceed to the next element.
            /// </summary>
            /// <returns>Boolean indicating whether or not this enumerator can proceed to the next element.</returns>
            public bool HasNext() {
                return Position < _list.Count - 1;
            }

            #endregion

            #region IEnumerator

            /// <summary>
            /// The current item.
            /// </summary>
            public SabberStoneAction Current {
                get {
                    if (Position >= 0 && Position < _list.Count) return _list[Position];
                    else throw new InvalidOperationException();
                }
            }

            /// <summary>
            /// The current item.
            /// </summary>
            object IEnumerator.Current => Current;

            /// <summary>
            /// Disposes this object's resources. Since this class is sealed this has no implementation.
            /// </summary>
            public void Dispose() {
            }

            #endregion

            #region IEnumerable

            /// <summary>
            /// Returns the enumerator.
            /// </summary>
            /// <returns>Enumerator.</returns>
            public IEnumerator<SabberStoneAction> GetEnumerator() {
                return this;
            }

            /// <summary>
            /// Advances the position of the enumerator.
            /// </summary>
            /// <returns>Boolean indicating whether or not a next item is available.</returns>
            public bool MoveNext() {
                Position++;
                return Position < _list.Count;
            }

            /// <summary>
            /// Resets the position of the enumerator back to the starting position (<see cref="STARTING_POSITION"/>).
            /// </summary>
            public void Reset() {
                Position = STARTING_POSITION;
            }

            /// <summary>
            /// Returns the enumerator.
            /// </summary>
            /// <returns>Enumerator.</returns>
            IEnumerator IEnumerable.GetEnumerator() {
                return this;
            }

            #endregion

        }

        #endregion

        #region Constants

        private const double PLAYER_WIN_SCORE = 1.0;
        private const double PLAYER_LOSS_SCORE = 0.0;

        #endregion

        #region Properties

        public bool HierarchicalExpansion { get; }
        public IGoalStrategy<List<SabberStoneAction>, SabberStoneState, SabberStoneAction, object, SabberStoneAction> Goal { get; }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructs a new instance of SabberStoneGameLogic.
        /// </summary>
        /// <param name="hierarchicalExpansion">Whether or not expansion should be handled hierarchically.</param>
        /// <param name="goal">The goal strategy.</param>
        public SabberStoneGameLogic(bool hierarchicalExpansion, IGoalStrategy<List<SabberStoneAction>, SabberStoneState, SabberStoneAction, object, SabberStoneAction> goal) {
            HierarchicalExpansion = hierarchicalExpansion;
            Goal = goal;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Applies a SabberStoneAction to a SabberStoneState which results in a new SabberStoneState.
        /// </summary>
        /// <param name="context">The context of the search.</param>
        /// <param name="position">The state to which the action should be applied.</param>
        /// <param name="action">The action to apply.</param>
        /// <returns>SabberStoneState that is the result of applying the action.</returns>
        public SabberStoneState Apply(SearchContext<List<SabberStoneAction>, SabberStoneState, SabberStoneAction, object, SabberStoneAction> context, SabberStoneState position, SabberStoneAction action) {

            // In case of hierarchical expansion, we'll arrive here with incomplete actions, i.e. actions that might not have end-turn tasks.
            // We'll still have to process these actions to move the state into a new state, ready for further expansion.

            // Check if the action is complete, or if we are applying Hierarchical Expansion (in that case the action will be incomplete).
            if (action.IsComplete() || HierarchicalExpansion) {

                // Process each task.
                foreach (var item in action.Tasks) {
                    position.Game.Process(item.Task);
                }
            } else {

                // In the case of an incomplete action, just pass the turn.
                position.Game.Process(EndTurnTask.Any(position.Game.CurrentPlayer));
            }

            // Return the position.
            return position;
        }

        /// <summary>
        /// Determines if a SabberStoneState represents a completed game.
        /// </summary>
        /// <param name="context">The context of the search.</param>
        /// <param name="position">The game state.</param>
        /// <returns>Whether or not the game is completed.</returns>
        public bool Done(SearchContext<List<SabberStoneAction>, SabberStoneState, SabberStoneAction, object, SabberStoneAction> context, SabberStoneState position) {
            return Goal.Done(context, position);
        }

        /// <summary>
        /// Expand the search from a SabberStoneState.
        /// </summary>
        /// <param name="context">The context of the search.</param>
        /// <param name="position">The state to expand from.</param>
        /// <returns>An enumeration of possible actions from the argument state.</returns>
        public IPositionGenerator<SabberStoneAction> Expand(SearchContext<List<SabberStoneAction>, SabberStoneState, SabberStoneAction, object, SabberStoneAction> context, SabberStoneState position) {
            var availableActionSequences = new List<SabberStoneAction>();
            var activePlayer = position.Game.CurrentPlayer;
            var activePlayerId = activePlayer.Id;

            //TODO: not create all options at once when expanding, but one by one as the iterator asks for them

            // When expanding on a position, we'll have a number of tasks to choose from.
            // The problem is that each task isn't exclusive with other tasks and/or may lead to more available tasks when processed.

            // So one thing we can do is make an action that has each of the currently available tasks as its first task.
            var topLevelActions = new List<SabberStoneAction>();
            var availableOptions = activePlayer.Options();
            availableOptions = availableOptions.Where(i => i.ZonePosition <= 0).ToList();
            foreach (var item in availableOptions) {
                var action = new SabberStoneAction();
                action.AddTask((SabberStonePlayerTask)item);
                topLevelActions.Add(action);
            }

            // If we are expanding hierarchically we can just return the top level actions.
            if (HierarchicalExpansion) return new SabberStoneMoveGenerator(topLevelActions);

            // If we are not expanding hierarchically, we'll need to generate all action sequences at once.

            // Recursively expand the available actions.
            foreach (var action in topLevelActions) {
                ExpandAction(position, action, activePlayerId, ref availableActionSequences);
            }

            // Return a move generator based on the list of available action sequences.
            return new SabberStoneMoveGenerator(availableActionSequences);
        }

        /// <summary>
        /// Calculate the scores of a SabberStoneState.
        /// </summary>
        /// <param name="position">The SabberStoneState.</param>
        /// <returns>Array of Double containing the scores per player, indexed by player ID.</returns>
        public double[] Scores(SabberStoneState position) => new[] {
            position.PlayerWon == position.Player1.Id ? PLAYER_WIN_SCORE : PLAYER_LOSS_SCORE,
            position.PlayerWon == position.Player2.Id ? PLAYER_WIN_SCORE : PLAYER_LOSS_SCORE
        };

        #endregion

        #region Private Methods

        /// <summary>
        /// Recursively expands an action by creating new actions and adding possible tasks to them.
        /// </summary>
        /// <param name="rootState">The game state at the root of the recursive call.</param>
        /// <param name="action">The action.</param>
        /// <param name="playerId">The unique identifier of the player that will play the action.</param>
        /// <param name="completeActions">A reference to a collection of completely expanded actions.</param>
        private void ExpandAction(SabberStoneState rootState, SabberStoneAction action, int playerId, ref List<SabberStoneAction> completeActions) {

            // If the latest option added was the end-turn task, return
            if (action.IsComplete()) {
                completeActions.Add(action);
                return;
            }

            // Go through all available options
            foreach (var option in CreateCurrentOptions(rootState, action, playerId)) {
                // Create a new action that is a copy of the current action
                var nextLevelAction = new SabberStoneAction(action.Tasks);
                // Add that task as the latest task
                nextLevelAction.AddTask(option);
                // Recursively call this method to find all combinations
                ExpandAction(rootState, nextLevelAction, playerId, ref completeActions);
            }
        }

        /// <summary>
        /// Creates the currently available options for a player after a specific action is executed.
        /// </summary>
        /// <param name="rootState">The game state before any tasks in the action are processed.</param>
        /// <param name="action">The action containing a list of tasks to process.</param>
        /// <param name="playerId">The unique identifier of the player that will play the action.</param>
        /// <param name="ignorePositioning">[Optional] Whether or not to treat minion play tasks with different positions as the same and ignore the extras. Default is true.</param>
        /// <returns>Collection of available tasks, potentially having minion play tasks with different positions filtered out.</returns>
        private static IEnumerable<SabberStonePlayerTask> CreateCurrentOptions(SabberStoneState rootState, SabberStoneAction action, int playerId, bool ignorePositioning = true) {

            // Clone the root state before tampering with it
            var clonedState = (SabberStoneState)rootState.Copy();
            // Apply the tasks in the current action to the root state
            foreach (var task in action.Tasks) {
                clonedState.Game.Process(task.Task);
            }

            // If it's no longer our player's turn, or if the game has ended return an empty list
            // Note: this will happen if the last task processed was an end-turn task or if that task ended the game
            if (clonedState.Game.CurrentPlayer.Id != playerId || clonedState.Game.State == State.COMPLETE) return new List<SabberStonePlayerTask>();

            // Query the game for the currently available actions
            var currentOptions = clonedState.Game.CurrentPlayer.Options();

            if (ignorePositioning) {
                // Filter out all tasks that have position set to anything higher than 0
                // Note: a position of -1 means that the task doesn't care about positioning, so those are left in
                currentOptions = currentOptions.Where(i => i.ZonePosition <= 0).ToList();
            }

            // Return the options
            return currentOptions.Select(i => (SabberStonePlayerTask)i);
        }

        #endregion

    }
}
