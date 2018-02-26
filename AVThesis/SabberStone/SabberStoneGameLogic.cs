using System;
using AVThesis.Datastructures;
using AVThesis.Game;
using AVThesis.Search;

/// <summary>
/// Written by A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by G.J. Roelofs and T. Aliyev.
/// </summary>
namespace AVThesis.SabberStone {

    public class SabberStoneGameLogic : IGameLogic<object, SabberStoneState, SabberStoneAction, object, SabberStoneAction, SabberStoneAction> {

        public SabberStoneState Apply(SearchContext<object, SabberStoneState, SabberStoneAction, object, SabberStoneAction> context, SabberStoneState position, SabberStoneAction action) {
            throw new NotImplementedException();
        }

        public bool Done(SearchContext<object, SabberStoneState, SabberStoneAction, object, SabberStoneAction> context, SabberStoneState position) {
            throw new NotImplementedException();
        }

        public IPositionGenerator<SabberStoneAction> Expand(SearchContext<object, SabberStoneState, SabberStoneAction, object, SabberStoneAction> context, SabberStoneState position) {
            throw new NotImplementedException();
        }

        public double[] Scores(SabberStoneState position) {
            throw new NotImplementedException();
        }
    }
}
