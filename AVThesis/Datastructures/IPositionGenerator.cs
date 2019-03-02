using System.Collections.Generic;

namespace AVThesis.Datastructures {

    public interface IPositionGenerator<out T> : IEnumerable<T>, IEnumerator<T> {

        bool HasNext();

    }
}
