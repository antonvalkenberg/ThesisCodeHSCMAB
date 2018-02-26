using System.Collections.Generic;

namespace AVThesis.Datastructures {

    public interface IPositionGenerator<T> : IEnumerable<T>, IEnumerator<T> {

        bool HasNext();

    }
}
