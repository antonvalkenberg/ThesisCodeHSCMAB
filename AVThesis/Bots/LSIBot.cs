using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AVThesis.SabberStone;
using SabberStoneCore.Model.Entities;

/// <summary>
/// Written by A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by G.J. Roelofs and T. Aliyev.
/// </summary>
namespace AVThesis.Bots {

    /// <inheritdoc />
    public class LSIBot : ISabberStoneBot {

        #region Constants

        private const int LSI_SAMPLES_FOR_GENERATION = 250;
        private const int LSI_SAMPLES_FOR_EVALUATION = 750;

        #endregion

        #region Fields

        private const string _botName = "LSIBot";

        #endregion

        #region Properties

        /// <summary>
        /// The player this bot is representing in a game of SabberStone.
        /// </summary>
        public Controller Player { get; set; }

        /// <summary>
        /// The bot that is used during the playouts.
        /// </summary>
        public ISabberStoneBot PlayoutBot { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructs a new instance of LSIBot with a <see cref="Controller"/> representing the player.
        /// </summary>
        /// <param name="player">The player.</param>
        public LSIBot(Controller player) {
            Player = player;

            // Set the playout bot

            // LSI will need a goal-strategy to determine when a simulation is done
            // LSI will need a playout-strategy to run the simulations
            // LSI will need an evaluation-strategy to evaluate the strength of samples


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
        /// <param name="state">The current search state.</param>
        /// <param name="samplesForGeneration">The number of samples allowed during the generation phase.</param>
        /// <param name="samplesForEvaluation">The number of samples allowed during the evaluation phase.</param>
        /// <returns></returns>
        private Object Generate(SabberStoneState state, int samplesForGeneration, int samplesForEvaluation) {

            // Create the Side Information using the allowed number of generation samples.

            // Create combined-actions by sampling the side information.

            throw new NotImplementedException();
        }

        /// <summary>
        /// Produces the side info, a list of distributions for individual actions in dimensions to an average score.
        /// </summary>
        /// <param name="state">The current search state.</param>
        /// <param name="samplesForGeneration">The number of samples allowed during the generation phase.</param>
        /// <returns></returns>
        private Object SideInformation(SabberStoneState state, int samplesForGeneration) {

            // So we have an issue here, because it's Hearthstone we don't know in advance how many dimensions we have.
            //      -> We can just evenly distribute budget over the currently available dimensions
            //      -- Reason is that some dimensions would be `locked' until after a specific dimensions is explored
            //      -> Another option is to expand all possible sequences, this might be too complex though...

            // In Hearthstone we don't really have multiple available actions per dimension
            // It's more like you either play/attack or not

            // We can still try to distilate individual actions
            // It might be best to keep track of these through a dictionary/array
            //      So we'd randomly choose actions and simulate when `end-turn' is chosen
            //      And then update the value of any selected PlayerTask

            // I guess use OddmentTable again?

            throw new NotImplementedException();
        }

        /// <summary>
        /// Select the best combined-action from C*.
        /// </summary>
        /// <param name="state">The current search state.</param>
        /// <param name="samplesForEvaluation">The number of samples allowed during the evaluation phase.</param>
        /// <param name="generatedActions">C*, the subset of combined-actions generated in the generation phase.</param>
        /// <returns></returns>
        private Object Evaluate(SabberStoneState state, int samplesForEvaluation, Object generatedActions) {

            // Create some way to track the value of each combined action

            // Create a function/routine that goes through a set of actions, evaluates them and returns the best half

            // Determine the number of iterations of this `Sequential Halving` routine we need to do

            // Return the resulting best action.

            throw new NotImplementedException();
        }

        #endregion

        #region Public Methods

        #region ISabberStoneBot

        /// <inheritdoc />
        public SabberStoneAction Act(SabberStoneState state) {
            // Keep track of the number of iterations used for generation and evaluation
            // We need to do this to empirically adjust the budget, since LSI tends to use more than allowed

            // LSI divides the search process into two separate phases

            // Generate a subset (C*) from all possible combined-actions (C)

            // Evaluate the best combined-action in C*


            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void SetController(Controller controller) {
            Player = controller;
        }

        /// <inheritdoc />
        public int PlayerID() {
            return Player.Id;
        }

        /// <inheritdoc />
        public string Name() {
            return _botName;
        }

        #endregion

        #endregion

    }
}
