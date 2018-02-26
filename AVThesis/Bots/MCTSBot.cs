using System;
using AVThesis.Game;
using AVThesis.SabberStone;
using AVThesis.SabberStone.Strategies;
using AVThesis.Search;
using AVThesis.Search.Tree;
using AVThesis.Search.Tree.MCTS;
using SabberStoneCore.Model.Entities;
using static AVThesis.Search.SearchContext<object, AVThesis.SabberStone.SabberStoneState, AVThesis.SabberStone.SabberStoneAction, object, AVThesis.SabberStone.SabberStoneAction>;

/// <summary>
/// Written by A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by G.J. Roelofs and T. Aliyev.
/// </summary>
namespace AVThesis.Bots {

    /// <summary>
    /// A bot that plays Hearthstone using Monte Carlo Tree Search to find its best move.
    /// </summary>
    public class MCTSBot : ISabberStoneBot {

        #region Constants

        private const int MCTS_NUMBER_OF_ITERATIONS = 1000;
        private const int MIN_T_VISIT_THRESHOLD_FOR_EXPANSION = 20;
        private const int SELECTION_VISIT_MINIMUM_FOR_EVALUATION = 50;
        private const double UCT_C_CONSTANT_DEFAULT = 0.1;
        private const int PLAYOUT_TURN_CUTOFF = 10;

        #endregion

        #region Fields

        private const string _botName = "MCTSBot";
        private Controller _player;
        private ISabberStoneBot _playoutBot;
        private IGoalStrategy<object, SabberStoneState, SabberStoneAction, object, SabberStoneAction> _goal;
        private IGameLogic<object, SabberStoneState, SabberStoneAction, object, SabberStoneAction, SabberStoneAction> _gameLogic;
        private MCTSBuilder<object, SabberStoneState, SabberStoneAction, object, SabberStoneAction> _builder;

        #endregion

        #region Properties

        public Controller Player { get => _player; set => _player = value; }
        public ISabberStoneBot PlayoutBot { get => _playoutBot; set => _playoutBot = value; }
        public IGoalStrategy<object, SabberStoneState, SabberStoneAction, object, SabberStoneAction> Goal { get => _goal; set => _goal = value; }
        public IGameLogic<object, SabberStoneState, SabberStoneAction, object, SabberStoneAction, SabberStoneAction> GameLogic { get => _gameLogic; set => _gameLogic = value; }
        public MCTSBuilder<object, SabberStoneState, SabberStoneAction, object, SabberStoneAction> Builder { get => _builder; set => _builder = value; }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructs a new instance of MCTSBot with a <see cref="Controller"/> representing the player.
        /// </summary>
        /// <param name="player">The player.</param>
        public MCTSBot(Controller player) : this() {
            Player = player;
        }

        /// <summary>
        /// Constructs a new instance of MCTSBot with default strategies.
        /// </summary>
        public MCTSBot() {

            // Note: we're not setting the Controller here, because we want to clone the current one when doing a playout.
            PlayoutBot = new RandomBot();

            // We'll be cutting off the simulations after X turns, using a GoalStrategy.
            Goal = new GoalStrategyTurnCutoff(PLAYOUT_TURN_CUTOFF);

            //GameLogic
            //TODO GameLogic

            //Playout
            //TODO PlayoutStrategy

            // Create the INodeEvaluation strategy used in the selection phase.
            var nodeEvaluation = new ScoreUCB<SabberStoneState, SabberStoneAction>(UCT_C_CONSTANT_DEFAULT);

            // Build MCTS
            Builder = MCTS<object, SabberStoneState, SabberStoneAction, object, SabberStoneAction>.Builder();
            Builder.ExpansionStrategy = new MinimumTExpansion<object, SabberStoneState, SabberStoneAction, object, SabberStoneAction>(MIN_T_VISIT_THRESHOLD_FOR_EXPANSION);
            Builder.SelectionStrategy = new BestNodeSelection<object, SabberStoneState, SabberStoneAction, object, SabberStoneAction>(SELECTION_VISIT_MINIMUM_FOR_EVALUATION, nodeEvaluation);
            Builder.EvaluationStrategy = new HearthStoneStateEvaluation();
            Builder.Iterations = MCTS_NUMBER_OF_ITERATIONS;
        }

        #endregion

        #region Public Methods
        
        #region ISabberStoneBot

        public SabberStoneAction Act(SabberStoneState state) {

            Console.WriteLine(Name());
            Console.WriteLine("Starting an MCTS-search in turn " + state.Game.Turn);

            // Setup a new search with the current state as source.
            SearchContext<object, SabberStoneState, SabberStoneAction, object, SabberStoneAction> context = GameSearchSetup(GameLogic, null, state.Copy(), null, Builder.Build());
            
            // Execute the search
            context.Execute();

            // Check if the search was successful
            if (context.Status != SearchStatus.Success) {
                // TODO throw exception, or print error.
                // TODO return random action.
            }
            
            return context.Solution;
        }

        public string Name() {
            return _botName;
        }

        public int PlayerID() {
            return Player.Id;
        }

        public void SetController(Controller controller) {
            Player = controller;
        }

        #endregion

        #endregion
    }
}
