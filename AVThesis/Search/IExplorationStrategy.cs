using System;

/// <summary>
/// Written by A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by G.J. Roelofs and T. Aliyev.
/// </summary>
namespace AVThesis.Search {

    /// <summary>
    /// Defines the necessities for a strategy that deals with exploring the search space.
    /// </summary>
    /// <typeparam name="D"><see cref="SearchContext{D}"/></typeparam>
    /// <typeparam name="P"><see cref="SearchContext{P}"/></typeparam>
    /// <typeparam name="A"><see cref="SearchContext{A}"/></typeparam>
    /// <typeparam name="S"><see cref="SearchContext{S}"/></typeparam>
    /// <typeparam name="Sol"><see cref="SearchContext{Sol}"/></typeparam>
    public interface IExplorationStrategy<D, P, A, S, Sol> where D : class where P : State where A : class where S : class where Sol : class {

        /// <summary>
        /// Determines whether the strategy calls for exploring or exploiting, based on the current iteration number.
        /// </summary>
        /// <param name="context">The context of the search.</param>
        /// <param name="currentIteration">The number of the current iteration of the search.</param>
        /// <returns>Boolean, indicating if the search should explore. True indicates explore, False indicates exploit.</returns>
        bool Policy(SearchContext<D, P, A, S, Sol> context, int currentIteration);

    }

    /// <summary>
    /// An Exploration Strategy that uses chance to determine whether to explore or exploit.
    /// </summary>
    /// <typeparam name="D"><see cref="SearchContext{D}"/></typeparam>
    /// <typeparam name="P"><see cref="SearchContext{P}"/></typeparam>
    /// <typeparam name="A"><see cref="SearchContext{A}"/></typeparam>
    /// <typeparam name="S"><see cref="SearchContext{S}"/></typeparam>
    /// <typeparam name="Sol"><see cref="SearchContext{Sol}"/></typeparam>
    public class ChanceExploration<D, P, A, S, Sol> : IExplorationStrategy<D, P, A, S, Sol> where D : class where P : State where A : class where S : class where Sol : class {

        #region Fields

        /// <summary>
        /// Random number generator.
        /// </summary>
        private readonly Random _rng = new Random();

        #endregion

        #region Properties

        /// <summary>
        /// The chance of exploration.
        /// </summary>
        public double ChanceToExplore { get; set; }

        #endregion

        #region Constructors
        
        /// <summary>
        /// Constructs a new instance of the ChanceExploration strategy with a specific chance to explore.
        /// </summary>
        /// <param name="chanceToExplore">The chance that this strategy indicates explore over exploit.</param>
        public ChanceExploration(double chanceToExplore) {
            ChanceToExplore = chanceToExplore;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Determines whether the strategy calls for exploring or exploiting, based on the current iteration number.
        /// </summary>
        /// <param name="context">The context of the search.</param>
        /// <param name="currentIteration">The number of the current iteration of the search.</param>
        /// <returns>Boolean, indicating if the search should explore. True indicates explore, False indicates exploit.</returns>
        public bool Policy(SearchContext<D, P, A, S, Sol> context, int currentIteration) {
            return _rng.NextDouble() <= ChanceToExplore;
        }

        #endregion

    }

}
