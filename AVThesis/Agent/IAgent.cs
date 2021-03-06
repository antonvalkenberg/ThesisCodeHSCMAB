﻿/// <summary>
/// Copyright © 2019 Anton Valkenberg
/// Written by BSc. A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by MSc. G.-J. Roelofs, MSc. T. Aliyev and MSc. D. de Rydt.
/// </summary>
namespace AVThesis.Agent {

    /// <summary>
    /// Defines what an agent should be able to do.
    /// </summary>
    /// <typeparam name="C">The Type of context the agent operates in.</typeparam>
    /// <typeparam name="S">The Type of state the agent should be able to process.</typeparam>
    /// <typeparam name="A">The Type of action the agent should perform.</typeparam>
    public interface IAgent<in C, in S, out A> {

        /// <summary>
        /// Return an action based on the current state.
        /// </summary>
        /// <param name="context">The context of the search.</param>
        /// <param name="state">The current state of the search.</param>
        /// <returns>An action.</returns>
        A Act(C context, S state);

    }
}
