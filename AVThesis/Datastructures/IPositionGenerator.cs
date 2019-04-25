using System.Collections.Generic;

/// <summary>
/// Copyright © 2019 Anton Valkenberg
/// Written by BSc. A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by MSc. G.-J. Roelofs, MSc. T. Aliyev and MSc. D. de Rydt.
/// </summary>
namespace AVThesis.Datastructures {

    public interface IPositionGenerator<out T> : IEnumerable<T>, IEnumerator<T> {

        bool HasNext();

    }
}
