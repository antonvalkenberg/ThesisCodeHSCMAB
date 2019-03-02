/// <summary>
/// Written by A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by G.J. Roelofs and T. Aliyev.
/// </summary>
namespace AVThesis.Search.LSI {

    /// <inheritdoc />
    public interface ILSISamplingStrategy<in P, out A, in T> : ISamplingStrategy<P, A> where P : State where A : class where T : class {

        A Sample(P state, T sideInformation);

    }
}
