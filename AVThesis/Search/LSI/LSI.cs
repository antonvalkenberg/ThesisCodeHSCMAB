using System;
using System.Collections.Generic;
using System.Linq;
using AVThesis.Datastructures;
using AVThesis.Game;

/// <summary>
/// Written by A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by G.J. Roelofs and T. Aliyev.
/// </summary>
namespace AVThesis.Search.LSI {

    /// <summary>
    /// Linear Side Information.
    /// </summary>
    /// <typeparam name="D">D in <see cref="SearchContext{D, P, A, S, Sol}"/></typeparam>
    /// <typeparam name="P">P in <see cref="SearchContext{D, P, A, S, Sol}"/></typeparam>
    /// <typeparam name="A">A in <see cref="SearchContext{D, P, A, S, Sol}"/></typeparam>
    /// <typeparam name="S">S in <see cref="SearchContext{D, P, A, S, Sol}"/></typeparam>
    /// <typeparam name="N">A Type of node that the search uses.</typeparam>
    /// <typeparam name="T">The Type of side information that the search uses.</typeparam>
    public class LSI<D, P, A, S, N, T> : ISearchStrategy<D, P, A, S, A> where D : class where P : State where A : class where S : class where N : Node<A>, new() where T : class {
        
        #region Helper Class

        private class ActionValue {
            public A Action { get; set; }
            public double Value { get; set; }

            public ActionValue(A action, double value) {
                Action = action;
                Value = value;
            }

            public override string ToString() {
                return $"{Action} -> {Value}";
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// The amount of samples to be used during the Generation phase.
        /// </summary>
        public int GenerationSamples { get; set; }

        /// <summary>
        /// The amount of samples to be used during the Evaluation phase.
        /// </summary>
        public int EvaluationSamples { get; set; }

        /// <summary>
        /// The table containing the side information.
        /// </summary>
        public T SideInformation { get; set; }

        /// <summary>
        /// The strategy used to create the side information.
        /// </summary>
        public ISideInformationStrategy<D, P, A, S, A, T> SideInformationStrategy { get; set; }

        /// <summary>
        /// A strategy to sample actions during the Generation process.
        /// </summary>
        public ILSISamplingStrategy<P, A, T> SamplingStrategy { get; set; }

        /// <summary>
        /// The strategy used to play out a game in simulation.
        /// </summary>
        public IPlayoutStrategy<D, P, A, S, A> Playout { get; set; }

        /// <summary>
        /// The evaluation strategy for determining the value of samples.
        /// </summary>
        public IStateEvaluation<D, P, A, S, A, N> Evaluation { get; set; }

        /// <summary>
        /// The game specific logic required for searching.
        /// </summary>
        public IGameLogic<D, P, A, S, A, A> GameLogic { get; set; }

        /// <summary>
        /// Used to keep track of the actual number of samples LSI uses during the evaluation phase.
        /// </summary>
        public int SamplesUsedEvaluation { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of a Linear Side Information search.
        /// </summary>
        /// <param name="generationSamples">The amount of samples to be used during the Generation phase.</param>
        /// <param name="evaluationSamples">The amount of samples to be used during the Evaluation phase.</param>
        /// <param name="sideInformationStrategy">The strategy used to create the side information.</param>
        /// <param name="samplingStrategy">A strategy to sample actions during the Generation process.</param>
        /// <param name="playout">The strategy used to play out a game in simulation.</param>
        /// <param name="evaluation">The evaluation strategy for determining the value of samples.</param>
        /// <param name="gameLogic">The game specific logic required for searching.</param>
        public LSI(int generationSamples, int evaluationSamples, ISideInformationStrategy<D, P, A, S, A, T> sideInformationStrategy, ILSISamplingStrategy<P, A, T> samplingStrategy, IPlayoutStrategy<D, P, A, S, A> playout, IStateEvaluation<D, P, A, S, A, N> evaluation, IGameLogic<D, P, A, S, A, A> gameLogic) {
            GenerationSamples = generationSamples;
            EvaluationSamples = evaluationSamples;
            SideInformationStrategy = sideInformationStrategy;
            SamplingStrategy = samplingStrategy;
            Playout = playout;
            Evaluation = evaluation;
            GameLogic = gameLogic;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Generates the interesting subset of actions C* from C.
        /// 
        /// 1) Generate a weight function R^ from PartialActions(adopting the linear side information assumption).
        /// 2) Schematically generating a probability distribution D_R^ over CombinedAction space C, biased "towards" R^.
        /// 3) Sample a number of CombinedActions C* from D_R^.
        /// </summary>
        /// <param name="context">The current search context.</param>
        /// <returns>List of <see cref="A"/>.</returns>
        private List<A> Generate(SearchContext<D, P, A, S, A> context) {

            // Create the Side Information using the allowed number of generation samples.
            SideInformation = SideInformationStrategy.Create(context, GenerationSamples);

            // Create combined-actions by sampling the side information.
            List<A> sampledActions = new List<A>();
            for (int i = 0; i < EvaluationSamples; i++) {
                sampledActions.Add(SamplingStrategy.Sample((P)context.Source.Copy(), SideInformation));
            }

            return sampledActions;
        }

        /// <summary>
        /// Select the best combined-action from C*.
        /// </summary>
        /// <param name="context">The current search context.</param>
        /// <param name="generatedActions">C*, the subset of combined-actions generated in the generation phase.</param>
        /// <returns>The action that is left after the Sequential Halving process, or null if there are none.</returns>
        private A Evaluate(SearchContext<D, P, A, S, A> context, List<A> generatedActions) {

            // Create some way to track the value of each combined action
            var actionValues = new List<ActionValue>();
            foreach (var action in generatedActions) {
                actionValues.Add(new ActionValue(action, 0));
            }

            // Determine the exact number of iterations of the halving routine we need to do to reduce the generated-actions to a singular item
            // For example : Ceiling(Log_2(100))=7 -> 100-50-25-13-7-4-2-1
            var timesToHalf = Math.Max(1, (int)Math.Ceiling(Math.Log(generatedActions.Count, 2)));

            for (var i = 0; i < timesToHalf; i++) {
                // Determine the number of samples that can be used per action during this iteration
                // For example, when we have 1000 samples to divide over 100 generated-actions,
                // the level of samples-per-action work out to be: 1-2-5-10-20-35-71, for a total of 877 samples
                // This also illustrates the need for Floor instead of Round or Ceiling
                var samplesPerAction = Math.Max(1, (int)Math.Floor(EvaluationSamples / (actionValues.Count * timesToHalf * 1.0)));

                actionValues = SelectBestHalf(context, actionValues, samplesPerAction);
            }

            // Return the resulting best action.
            return actionValues.IsNullOrEmpty() ? null : actionValues.First().Action;
        }

        /// <summary>
        /// Selects the best half of a collection of actions by evaluating them based on the provided search context.
        /// </summary>
        /// <param name="context">The current search context.</param>
        /// <param name="actions">The collection of actions to filter.</param>
        /// <param name="samplesPerAction">How many samples to run per action.</param>
        /// <returns>Collection of actions which Count is half of the original collection, rounded up. This collection is ordered by descending value.</returns>
        private List<ActionValue> SelectBestHalf(SearchContext<D, P, A, S, A> context, List<ActionValue> actions, int samplesPerAction) {

            // Evaluate each action by running a playout for it.
            foreach (var item in actions) {

                var tempNode = new N { Payload = item.Action };
                var newState = GameLogic.Apply(context, (P)context.Source.Copy(), item.Action);

                double value = 0;
                for (var i = 0; i < samplesPerAction; i++) {
                    var endState = Playout.Playout(context, (P)newState.Copy());
                    value += Evaluation.Evaluate(context, tempNode, endState);
                    SamplesUsedEvaluation++;
                }

                item.Value = value;
            }

            // Return the half with the highest value.
            var half = (int)Math.Max(1, Math.Ceiling(actions.Count / 2.0));
            return actions.OrderByDescending(i => i.Value).Take(half).ToList();
        }

        #endregion

        #region Public Methods

        public void Search(SearchContext<D, P, A, S, A> context) {
            // Let's keep track of how many samples LSI actually uses during evaluation.
            SamplesUsedEvaluation = 0;

            // LSI divides the search process into two separate phases

            // Generate a subset (C*) from all possible combined-actions (C)
            var cStar = Generate(context);

            // Evaluate and set the best combined-action in C* as solution to the search.
            context.Solution = Evaluate(context, cStar);

            context.Status = SearchContext<D, P, A, S, A>.SearchStatus.Success;
        }

        #endregion

    }

}
