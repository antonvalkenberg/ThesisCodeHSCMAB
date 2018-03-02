using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AVThesis.Datastructures;
using AVThesis.Game;
using AVThesis.Search;
using SabberStoneCore.Model.Entities;
using SabberStoneCore.Tasks.PlayerTasks;

/// <summary>
/// Written by A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by G.J. Roelofs and T. Aliyev.
/// </summary>
namespace AVThesis.SabberStone {

    /// <summary>
    /// Handles the game specific logic required for search in SabberStone.
    /// </summary>
    public class SabberStoneGameLogic : IGameLogic<object, SabberStoneState, SabberStoneAction, object, SabberStoneAction, SabberStoneAction> {

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

        #region Constructor

        /// <summary>
        /// Constructs a new instance of SabberStoneGameLogic.
        /// </summary>
        public SabberStoneGameLogic() {
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
        public SabberStoneState Apply(SearchContext<object, SabberStoneState, SabberStoneAction, object, SabberStoneAction> context, SabberStoneState position, SabberStoneAction action) {
            // Check if the action is valid.
            if (action.IsValid()) {

                // Process each task.
                foreach (var item in action.Tasks) {
                    position.Game.Process(item);
                }
            } else {

                // In the case of an invalid action, just pass the turn.
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
        public bool Done(SearchContext<object, SabberStoneState, SabberStoneAction, object, SabberStoneAction> context, SabberStoneState position) {
            return position.Game.State == SabberStoneCore.Enums.State.COMPLETE;
        }

        /// <summary>
        /// Expand the search from a SabberStoneState.
        /// </summary>
        /// <param name="context">The context of the search.</param>
        /// <param name="position">The state to expand from.</param>
        /// <returns>An enumeration of possible actions from the argument state.</returns>
        public IPositionGenerator<SabberStoneAction> Expand(SearchContext<object, SabberStoneState, SabberStoneAction, object, SabberStoneAction> context, SabberStoneState position) {
            List<SabberStoneAction> availableActionSequences = new List<SabberStoneAction>();
            Controller activePlayer = position.Game.CurrentPlayer;
            int activePlayerId = activePlayer.Id;
            EndTurnTask endTurnTask = EndTurnTask.Any(activePlayer);
            
            // When expanding on a position, we'll have a number of tasks to choose from
            // The problem is, each task isn't exclusive with other tasks, or may lead to more available tasks

            // So one thing we can do is make an action that had each of the current available tasks as its first task
            List<SabberStoneAction> topLevelActions = new List<SabberStoneAction>();
            foreach (var item in activePlayer.Options()) {
                var action = new SabberStoneAction();
                action.AddTask(item);
                topLevelActions.Add(action);
            }

            // Then recursively expand these actions.
            foreach (var action in topLevelActions) {
                ExpandAction(position, action, activePlayerId, ref availableActionSequences);
            }
            
            // Return a move generator based on the list of available actions.
            return new SabberStoneMoveGenerator(availableActionSequences);
        }

        /// <summary>
        /// Calculate the scores of a SabberStoneState.
        /// </summary>
        /// <param name="position">The game state.</param>
        /// <returns>Array of Double containing the scores per player. Index 0 represents Player1's score.</returns>
        public double[] Scores(SabberStoneState position) {
            return new double[] {
                position.PlayerWon == position.Player1.Id ? PLAYER_WIN_SCORE : PLAYER_LOSS_SCORE,
                position.PlayerWon == position.Player2.Id ? PLAYER_WIN_SCORE : PLAYER_LOSS_SCORE
            };
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Recursively expands an action by creating new actions and adding possible tasks to them.
        /// </summary>
        /// <param name="state">The game state on which the action should be applied.</param>
        /// <param name="action">The action.</param>
        /// <param name="playerID">The unique identifier of the player that will play the action.</param>
        /// <param name="completeActions">A reference to a collection of completely expanded actions.</param>
        private void ExpandAction(SabberStoneState state, SabberStoneAction action, int playerID, ref List<SabberStoneAction> completeActions) {
            // Get the latest task
            var latestTask = action.Tasks.Last();
            // Clone the current position before processing any task
            var clonedState = (SabberStoneState)state.Copy();
            // Process the latest task on the cloned state
            clonedState.Game.Process(latestTask);

            // Check if our player is still the current player
            if (clonedState.Game.CurrentPlayer.Id == playerID) {
                // Go through all options
                foreach (var option in clonedState.Game.CurrentPlayer.Options()) {
                    // Create a new action that is a copy of the current action
                    var nextLevelAction = new SabberStoneAction(action.Tasks);
                    // Add that task as the latest task
                    nextLevelAction.AddTask(option);
                    // Recursively call this method to find all combinations
                    ExpandAction(clonedState, nextLevelAction, playerID, ref completeActions);
                }
            } else {
                // Add current item to a list of completed actions, if it is a valid action
                if (action.IsValid()) completeActions.Add(action);
                // Stop recursion and return
                return;
            }
        }

        #endregion

    }
}
