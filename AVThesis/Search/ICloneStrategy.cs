/// <summary>
/// Written by A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by G.J. Roelofs and T. Aliyev.
/// </summary>
namespace AVThesis.Search {

    /// <summary>
    /// Defines what a strategy should have to clone objects.
    /// </summary>
    /// <typeparam name="T">The Type of Object that needs to be cloned.</typeparam>
    public interface ICloneStrategy<T> {

        /// <summary>
        /// Clones a provided object.
        /// </summary>
        /// <param name="toClone">The object to clone.</param>
        /// <returns>A cloned instance of the object.</returns>
        T Clone(T toClone);

    }

    /// <summary>
    /// Clones objects descending from State by using the <see cref="State.Copy{T}"/> method.
    /// </summary>
    /// <typeparam name="T">Type that has State as its base class.</typeparam>
    public class StateCloner<T> : ICloneStrategy<T> where T : State {

        /// <summary>
        /// Clones the provided object if it has the class State as a base class, otherwise returns the object itself.
        /// </summary>
        /// <param name="toClone">The object to clone.</param>
        /// <returns>Cloned instance of the object or the object itself.</returns>
        public T Clone(T toClone) {
            return (T) toClone.Copy();
        }

    }

}
