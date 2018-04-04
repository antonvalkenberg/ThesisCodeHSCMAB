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
    public class SabberStoneAction : IMove {

        #region Fields

        private List<PlayerTask> _tasks;

        #endregion

        #region Properties

        /// <summary>
        /// The PlayerTasks that this SabberStoneAction represents.
        /// </summary>
        public List<PlayerTask> Tasks { get => _tasks; private set => _tasks = value; }

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
        /// Note: if this action is invalid, -1 will be returned.
        /// </summary>
        /// <returns>Integer representing the unique identifier of the player that plays this SabberStoneAction.</returns>
        public int Player() {
            if (!IsValid()) return -1;
            return Tasks.First().Controller.Id;
        }

        /// <summary>
        /// Add a PlayerTask to the end of this SabberStoneAction's action list.
        /// </summary>
        /// <param name="task">The task to be added.</param>
        public void AddTask(PlayerTask task) {
            Tasks.Add(task);
        }

        /// <summary>
        /// Checks whether or not this SabberStoneAction is valid.
        /// Currently checks for: non-empty, ending with END_TURN.
        /// </summary>
        /// <returns></returns>
        public bool IsValid() {
            // An action is valid if it's not empty.
            if (Tasks.IsNullOrEmpty()) return false;
            // An action is valid if it ends with passing the turn.
            return Tasks.Last().PlayerTaskType == PlayerTaskType.END_TURN;
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
