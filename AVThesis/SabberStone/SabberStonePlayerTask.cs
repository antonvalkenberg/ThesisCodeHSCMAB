using SabberStoneCore.Tasks;

/// <summary>
/// Written by A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by G.J. Roelofs and T. Aliyev.
/// </summary>
namespace AVThesis.SabberStone {

    /// <summary>
    /// Wrapper class for <see cref="PlayerTask"/>.
    /// </summary>
    public class SabberStonePlayerTask {

        public static explicit operator SabberStonePlayerTask(PlayerTask t) {
            return new SabberStonePlayerTask(t);
        }

        #region Properties

        /// <summary>
        /// The task that this <see cref="SabberStonePlayerTask"/> wraps.
        /// </summary>
        public PlayerTask Task { get; }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new <see cref="SabberStonePlayerTask"/>.
        /// </summary>
        /// <param name="task">The task to wrap.</param>
        public SabberStonePlayerTask(PlayerTask task) {
            Task = task;
        }

        #endregion

        #region Public Methods

        #region Overridden Methods

        public override int GetHashCode() {
            //TODO create correct implementation of PlayerTask.GetHashCode()
            return Task.FullPrint().GetHashCode();
        }

        public override string ToString() {
            return Task.FullPrint();
        }
        #endregion

        #endregion


    }

}
