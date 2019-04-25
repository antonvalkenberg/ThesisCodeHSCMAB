using System;

/// <summary>
/// Copyright © 2019 Anton Valkenberg
/// Written by BSc. A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by MSc. G.-J. Roelofs, MSc. T. Aliyev and MSc. D. de Rydt.
/// </summary>
namespace AVThesis.Search {

    /// <summary>
    /// Represents an exception during searching.
    /// </summary>
    [Serializable]
    public class SearchException : Exception {

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        public SearchException() {
        }

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="message">The message that explains the exception.</param>
        public SearchException(string message) : base(message) {
        }

        /// <summary>
        /// Creates a new instance.
        /// </summary>
        /// <param name="message">The message that explains the exception.</param>
        /// <param name="innerException">The exception that caused this exception to trigger.</param>
        public SearchException(string message, Exception innerException) : base(message, innerException) {
        }
    }

}
