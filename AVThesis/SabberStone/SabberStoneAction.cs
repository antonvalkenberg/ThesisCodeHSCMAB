using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AVThesis.Datastructures;
using AVThesis.Game;
using SabberStoneCore.Model.Entities;
using SabberStoneCore.Tasks;

/// <summary>
/// Written by A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by G.J. Roelofs and T. Aliyev.
/// </summary>
namespace AVThesis.SabberStone {

    /// <summary>
    /// Implementation of the IMove interface for using a PlayerTask from a SabberStone Game as an action in the search framework.
    /// </summary>
    public class SabberStoneAction : IMove, IEquatable<SabberStoneAction> {

        #region Properties

        /// <summary>
        /// The PlayerTasks that this SabberStoneAction represents.
        /// </summary>
        public List<PlayerTask> Tasks { get; }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructs a new instance of a SabberStoneAction.
        /// </summary>
        /// <param name="action">The PlayerTasks that the SabberStoneAction represents.</param>
        public SabberStoneAction(List<PlayerTask> action) {
            Tasks = new List<PlayerTask>(action);
        }

        /// <summary>
        /// Constructs a new instance of a SabberStoneAction with an empty action.
        /// </summary>
        public SabberStoneAction() {
            Tasks = new List<PlayerTask>();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Returns the unique identifier of the player that this SabberStoneAction belongs to.
        /// Note: if this action is empty, -1 will be returned.
        /// </summary>
        /// <returns>Integer representing the unique identifier of the player that plays this SabberStoneAction.</returns>
        public int Player() {
            if (Tasks.IsNullOrEmpty()) return -1;
            return Tasks.First().Controller.Id;
        }

        /// <summary>
        /// Add a PlayerTask to this SabberStoneAction's action list.
        /// </summary>
        /// <param name="task">The task to be added.</param>
        /// <param name="index">[Optional] The index to add the task at. Default value is -1.</param>
        public void AddTask(PlayerTask task, int index = -1) {
            if (index > -1) Tasks.Insert(index, task);
            else Tasks.Add(task);
        }

        /// <summary>
        /// Checks whether or not this SabberStoneAction is complete.
        /// Currently checks for: non-empty, ending with END_TURN.
        /// </summary>
        /// <returns>Whether or not this SabberStoneAction is complete.</returns>
        public bool IsComplete() {
            // An action is complete if it's not empty and ends with an END_TURN task.
            return !Tasks.IsNullOrEmpty() && Tasks.Last().PlayerTaskType == PlayerTaskType.END_TURN;
        }

        /// <summary>
        /// Creates a 'null' move, i.e. a move that passes without any action.
        /// </summary>
        /// <param name="player">The player to create the move for.</param>
        /// <returns>SabberStoneAction containing only an <see cref="SabberStoneCore.Tasks.PlayerTasks.EndTurnTask"/>.</returns>
        public static SabberStoneAction CreateNullMove(Controller player) {
            var nullMove = new SabberStoneAction();
            nullMove.AddTask(SabberStoneCore.Tasks.PlayerTasks.EndTurnTask.Any(player));
            return nullMove;
        }

        #endregion

        #region Overridden Methods

        public static bool operator ==(SabberStoneAction left, SabberStoneAction right) {
            return Equals(left, right);
        }

        public static bool operator !=(SabberStoneAction left, SabberStoneAction right) {
            return !Equals(left, right);
        }

        public override bool Equals(object obj) {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((SabberStoneAction)obj);
        }

        public bool Equals(SabberStoneAction other) {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;
            return GetHashCode() == other.GetHashCode();
        }

        public override int GetHashCode() {
            unchecked { // overflow is fine, the number just wraps
                var hash = (int)Constants.HASH_OFFSET_BASIS;
                foreach (var playerTask in Tasks) {
                    hash = Constants.HASH_FNV_PRIME * (hash ^ playerTask.GetHashCode());
                }
                return hash;
            }
        }

        /// <summary>
        /// Returns a string representation of this SabberStoneAction.
        /// </summary>
        /// <returns>String representing this SabberStoneAction.</returns>
        public override string ToString() {
            var sb = new StringBuilder();
            sb.AppendLine($"SabberStoneAction for player with ID {Player()}, containing {Tasks.Count} task(s).");
            foreach (var item in Tasks) {
                sb.AppendLine(item.FullPrint());
            }
            return sb.ToString();
        }

        #endregion

    }
}
