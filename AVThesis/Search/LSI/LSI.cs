using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        #region Constants

        /// <summary>
        /// The default percentage of the computation budget of LSI that is used during the generation phase.
        /// </summary>
        public const double DEFAULT_BUDGET_GENERATION_PERCENTAGE = 0.25;

        /// <summary>
        /// The default adjustment factor applied to the evaluation budget for LSI.
        /// Note: this factor should be empirically determined due to the nature of the SequentialHalving algorithm used during LSI evaluation phase.
        /// </summary>
        public const double DEFAULT_EVALUATION_SAMPLES_ADJUSTMENT_FACTOR = .4;

        /// <summary>
        /// The amount of playouts to run when determining sample sizes when on a time budget.
        /// </summary>
        public const int TIME_BUDGET_TEST_PLAYOUTS = 25;

        /// <summary>
        /// The factor with which to multiply a single sample's time when determining sample sizes on a time budget.
        /// </summary>
        public const double TIME_BUDGET_SAFETY_MARGIN = 1.5;

        #endregion

        #region Helper Class

        private class ActionValue {
            public A Action { get; }
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
        /// The amount of time in milliseconds that the search is allowed to run for.
        /// </summary>
        public long Time { get; set; }

        /// <summary>
        /// The amount of samples that the search is allowed to use.
        /// </summary>
        public int Samples { get; set; }

        /// <summary>
        /// The portion of samples to be spent during the generation phase.
        /// </summary>
        public double GenerationBudgetPortion { get; set; }

        /// <summary>
        /// The adjustment factor needed to keep the samples used during the evaluation phase within budget.
        /// </summary>
        public double EvaluationBudgetAdjustment { get; set; }

        /// <summary>
        /// The amount of samples to be used during the Generation phase.
        /// </summary>
        public int GenerationSamples { get; set; }

        /// <summary>
        /// The amount of samples to be used during the Evaluation phase.
        /// </summary>
        public int EvaluationSamples { get; set; }

        /// <summary>
        /// Used to keep track of the actual number of samples LSI uses during the evaluation phase.
        /// </summary>
        public int SamplesUsedEvaluation { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of a Linear Side Information search.
        /// </summary>
        /// <param name="sideInformationStrategy">The strategy used to create the side information.</param>
        /// <param name="samplingStrategy">A strategy to sample actions during the Generation process.</param>
        /// <param name="playout">The strategy used to play out a game in simulation.</param>
        /// <param name="evaluation">The evaluation strategy for determining the value of samples.</param>
        /// <param name="gameLogic">The game specific logic required for searching.</param>
        /// <param name="time">[Optional] The time budget for this search. Default value is <see cref="Constants.NO_LIMIT_ON_THINKING_TIME"/>.</param>
        /// <param name="samples">[Optional] The iteration budget for this search. Default value is <see cref="Constants.NO_LIMIT_ON_ITERATIONS"/>.</param>
        /// <param name="generationBudgetPortion">[Optional] The portion of samples to be spent during the generation phase. Default value is <see cref="DEFAULT_BUDGET_GENERATION_PERCENTAGE"/>.</param>
        /// <param name="evaluationBudgetAdjustment">[Optional] The adjustment factor needed to keep the samples used during the evaluation phase within budget. Default value is <see cref="DEFAULT_EVALUATION_SAMPLES_ADJUSTMENT_FACTOR"/>.</param>
        public LSI(ISideInformationStrategy<D, P, A, S, A, T> sideInformationStrategy, ILSISamplingStrategy<P, A, T> samplingStrategy, IPlayoutStrategy<D, P, A, S, A> playout, IStateEvaluation<D, P, A, S, A, N> evaluation, IGameLogic<D, P, A, S, A, A> gameLogic, long time = Constants.NO_LIMIT_ON_THINKING_TIME, int samples = Constants.NO_LIMIT_ON_ITERATIONS, double generationBudgetPortion = DEFAULT_BUDGET_GENERATION_PERCENTAGE, double evaluationBudgetAdjustment = DEFAULT_EVALUATION_SAMPLES_ADJUSTMENT_FACTOR) {
            SideInformationStrategy = sideInformationStrategy;
            SamplingStrategy = samplingStrategy;
            Playout = playout;
            Evaluation = evaluation;
            GameLogic = gameLogic;
            Time = time;
            Samples = samples;
            GenerationBudgetPortion = generationBudgetPortion;
            EvaluationBudgetAdjustment = evaluationBudgetAdjustment;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Determines the sample sizes to be used for the search.
        /// <param name="context">The current search context.</param>
        /// </summary>
        private void DetermineSampleSizes(SearchContext<D, P, A, S, A> context) {
            
            // If we are on a time budget, run some number of test playouts to estimate the amount of samples we should use.
            if (Time != Constants.NO_LIMIT_ON_THINKING_TIME) {

                // Check how long it takes to run X playouts
                var timer = Stopwatch.StartNew();
                for (var i = 0; i < TIME_BUDGET_TEST_PLAYOUTS; i++) {
                    Playout.Playout(context, context.Source.Copy());
                }
                
                Console.WriteLine($"Running {TIME_BUDGET_TEST_PLAYOUTS} test playouts took {timer.ElapsedMilliseconds}ms");

                // Determine how long a single sample took
                var testDuration = timer.ElapsedMilliseconds;
                var sampleDuration = testDuration / (TIME_BUDGET_TEST_PLAYOUTS * 1.0);
                // Estimate how many samples we can run in the remaining time
                var estimatedSamples = (Time - testDuration) / (sampleDuration * TIME_BUDGET_SAFETY_MARGIN);
                Samples = (int) estimatedSamples;
            }

            GenerationSamples = (int)(Samples * GenerationBudgetPortion);
            EvaluationSamples = (int)(Samples * (1 - GenerationBudgetPortion) * EvaluationBudgetAdjustment);
        }

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
            var sampledActions = new List<A>();
            for (var i = 0; i < EvaluationSamples; i++) {
                sampledActions.Add(SamplingStrategy.Sample(context.Source, SideInformation));
            }

            return sampledActions;
        }

        /// <summary>
        /// Select the best combined-action from C*.
        /// </summary>
        /// <param name="context">The current search context.</param>
        /// <param name="generatedActions">C*, the subset of combined-actions generated in the generation phase.</param>
        /// <returns>The action that is left after the Sequential Halving process, or null if there are none.</returns>
        private A Evaluate(SearchContext<D, P, A, S, A> context, IReadOnlyCollection<A> generatedActions) {

            // Create some way to track the value of each combined action
            var actionValues = generatedActions.Select(action => new ActionValue(action, 0)).ToList();

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
        private List<ActionValue> SelectBestHalf(SearchContext<D, P, A, S, A> context, IReadOnlyCollection<ActionValue> actions, int samplesPerAction) {

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
            // Determine the samples sizes that should be used during each phase.
            DetermineSampleSizes(context);

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
