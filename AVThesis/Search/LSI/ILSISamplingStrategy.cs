/// <summary>
/// Copyright © 2019 Anton Valkenberg
/// Written by BSc. A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by MSc. G.-J. Roelofs, MSc. T. Aliyev and MSc. D. de Rydt.
/// </summary>
namespace AVThesis.Search.LSI {

    /// <inheritdoc />
    public interface ILSISamplingStrategy<in P, out A, in T> : ISamplingStrategy<P, A> where P : State where A : class where T : class {

        A Sample(P state, T sideInformation);

    }
}
