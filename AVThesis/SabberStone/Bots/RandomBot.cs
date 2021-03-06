﻿using System.Collections.Generic;
using System.Linq;
using AVThesis.Datastructures;
using AVThesis.Search;
using SabberStoneCore.Model.Entities;
using SabberStoneCore.Tasks.PlayerTasks;
using State = SabberStoneCore.Enums.State;

/// <summary>
/// Copyright © 2019 Anton Valkenberg
/// Written by BSc. A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by MSc. G.-J. Roelofs, MSc. T. Aliyev and MSc. D. de Rydt.
/// </summary>
namespace AVThesis.SabberStone.Bots {

    /// <summary>
    /// A bot that plays Hearthstone through random moves.
    /// </summary>
    public class RandomBot : IPlayoutBot {

        #region Constants

        private const string BOT_NAME = "RandomBot";

        #endregion

        #region Properties

        /// <summary>
        /// The player this bot represents.
        /// </summary>
        public Controller Player { get; set; }

        /// <summary>
        /// Whether or not to filter out excess positioning on playing cards.
        /// </summary>
        public bool FilterDuplicatePositionTasks { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructs a new instance of RandomBot with a <see cref="Controller"/> representing the player.
        /// </summary>
        /// <param name="player">The player.</param>
        /// <param name="filterDuplicatePositionTasks">[Optional] Whether or not to filter out excess positioning on playing cards. Default value is false.</param>
        public RandomBot(Controller player, bool filterDuplicatePositionTasks = false) : this(filterDuplicatePositionTasks) {
            Player = player;
        }

        /// <summary>
        /// Constructs a new instance of RandomBot without a set player.
        /// <param name="filterDuplicatePositionTasks">[Optional] Whether or not to filter out excess positioning on playing cards. Default value is false.</param>
        /// </summary>
        public RandomBot(bool filterDuplicatePositionTasks = false) {
            FilterDuplicatePositionTasks = filterDuplicatePositionTasks;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Creates a SabberStoneAction by randomly selecting one of the available PlayerTasks until the End_Turn task is selected.
        /// </summary>
        /// <param name="state">The game state for which an action should be created. Note: </param>
        /// <returns>SabberStoneAction</returns>
        public SabberStoneAction CreateRandomAction(SabberStoneState state) {
            // Clone game so that we can process the selected tasks and get an updated options list.
            var clonedGame = state.Game.Clone();
            var playerID = clonedGame.CurrentPlayer.Id;

            // Create an action to store the selected tasks.
            var action = new SabberStoneAction();

            // Keep adding random tasks until the player's turn is over, or the game has ended
            while (clonedGame.CurrentPlayer.Id == playerID && clonedGame.State != State.COMPLETE) {
                // Check if an duplicate positions need to be filtered out
                var availableOptions = clonedGame.CurrentPlayer.Options();
                if (FilterDuplicatePositionTasks)
                    availableOptions = availableOptions.Where(i => i.ZonePosition <= 0).ToList();
                // Select a random available task
                var selectedTask = availableOptions.RandomElementOrDefault();
                // Add the task to the action.
                action.AddTask((SabberStonePlayerTask)selectedTask);
                // Process the task on the cloned game state.
                clonedGame.Process(selectedTask);
            }

            // Check if the action is complete, if not, add EndTurn
            if (!action.IsComplete()) {
                action.AddTask((SabberStonePlayerTask)EndTurnTask.Any(clonedGame.CurrentPlayer));
            }

            return action;
        }

        /// <summary>
        /// Returns a SabberStoneAction for the current state.
        /// Note: If this player has no available options, null is returned.
        /// </summary>
        /// <param name="state">The current game state.</param>
        /// <returns>SabberStoneAction or null in the case of no available options.</returns>
        public SabberStoneAction Act(SabberStoneState state) {
            // Check to make sure the player to act in the game-state matches our player.
            if (state.CurrentPlayer() != Player.Id) {
                return null;
            }

            // Check if there are any options, otherwise return a randomly created action.
            return Player.Options().IsNullOrEmpty() ? SabberStoneAction.CreateNullMove(Player) : CreateRandomAction(state);
        }

        /// <summary>
        /// Sets the Controller that the bot represents within a SabberStone Game.
        /// </summary>
        /// <param name="controller">This bot's Controller.</param>
        public void SetController(Controller controller) {
            Player = controller;
        }

        /// <summary>
        /// Returns the player's ID.
        /// </summary>
        /// <returns>Integer representing the player's ID.</returns>
        public int PlayerID() {
            return Player.Id;
        }

        /// <summary>
        /// Returns the Bot's name.
        /// </summary>
        /// <returns>String representing the Bot's name.</returns>
        public string Name() {
            return Player == null ? BOT_NAME : $"{BOT_NAME}-{PlayerID()}";
        }

        /// <inheritdoc />
        public long BudgetSpent() {
            return 0;
        }

        /// <inheritdoc />
        public int MaxDepth() {
            return 0;
        }
        
        /// <inheritdoc />
        public void PlayoutCompleted(SearchContext<List<SabberStoneAction>, SabberStoneState, SabberStoneAction, object, SabberStoneAction> context, SabberStoneState endState) {
        }

        #endregion

    }
}
