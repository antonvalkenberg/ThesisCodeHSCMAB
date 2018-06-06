using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Configuration;
using AVThesis.Datastructures;
using AVThesis.Game;
using AVThesis.SabberStone;
using AVThesis.SabberStone.Strategies;
using AVThesis.Search;
using AVThesis.Search.Tree;
using AVThesis.Search.Tree.MCTS;
using SabberStoneCore.Model;
using SabberStoneCore.Model.Entities;
using SabberStoneCore.Tasks.PlayerTasks;
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

        private const int MCTS_NUMBER_OF_ITERATIONS = 10000;
        private const int MIN_T_VISIT_THRESHOLD_FOR_EXPANSION = 20;
        private const int SELECTION_VISIT_MINIMUM_FOR_EVALUATION = 50;
        private const double UCT_C_CONSTANT_DEFAULT = 0.1;
        private const int PLAYOUT_TURN_CUTOFF = 2;

        #endregion

        #region Fields

        private const string _botName = "MCTSBot";
        private Random _rng = new Random();
        private Controller _player;
        private ISabberStoneBot _playoutBot;
        private IGoalStrategy<object, SabberStoneState, SabberStoneAction, object, SabberStoneAction> _goal;
        private IGameLogic<object, SabberStoneState, SabberStoneAction, object, SabberStoneAction, SabberStoneAction> _gameLogic;
        private IPlayoutStrategy<object, SabberStoneState, SabberStoneAction, object, SabberStoneAction> _playout;
        private MCTSBuilder<object, SabberStoneState, SabberStoneAction, object, SabberStoneAction> _builder;
        private bool _hierarchicalExpansion;

        #endregion

        #region Properties

        /// <summary>
        /// The player this bot is representing in a game of SabberStone.
        /// </summary>
        public Controller Player { get => _player; set => _player = value; }

        /// <summary>
        /// The bot that is used during the playouts.
        /// </summary>
        public ISabberStoneBot PlayoutBot { get => _playoutBot; set => _playoutBot = value; }

        /// <summary>
        /// The strategy used to determine if a playout has reached its goal state.
        /// </summary>
        public IGoalStrategy<object, SabberStoneState, SabberStoneAction, object, SabberStoneAction> Goal { get => _goal; set => _goal = value; }

        /// <summary>
        /// The game specific logic required for searching through SabberStoneStates and SabberStoneActions
        /// </summary>
        public IGameLogic<object, SabberStoneState, SabberStoneAction, object, SabberStoneAction, SabberStoneAction> GameLogic { get => _gameLogic; set => _gameLogic = value; }

        /// <summary>
        /// The strategy used to play out a game in simulation.
        /// </summary>
        public IPlayoutStrategy<object, SabberStoneState, SabberStoneAction, object, SabberStoneAction> Playout { get => _playout; set => _playout = value; }

        /// <summary>
        /// The Monte Carlo Tree Search builder that creates a search-setup ready to use.
        /// </summary>
        public MCTSBuilder<object, SabberStoneState, SabberStoneAction, object, SabberStoneAction> Builder { get => _builder; set => _builder = value; }

        /// <summary>
        /// Whether or not to use Hierarchical Expansion during the search.
        /// </summary>
        public bool HierarchicalExpansion { get => _hierarchicalExpansion; set => _hierarchicalExpansion = value; }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructs a new instance of MCTSBot with a <see cref="Controller"/> representing the player.
        /// </summary>
        /// <param name="player">The player.</param>
        /// <param name="hierarchicalExpansion">[Optional] Whether or not to use Hierarchical Expansion. Default value is false.</param>
        public MCTSBot(Controller player, bool hierarchicalExpansion = false) : this(hierarchicalExpansion) {
            Player = player;
        }

        /// <summary>
        /// Constructs a new instance of MCTSBot with default strategies.
        /// </summary>
        /// <param name="hierarchicalExpansion">[Optional] Whether or not to use Hierarchical Expansion. Default value is false.</param>
        public MCTSBot(bool hierarchicalExpansion = false) {
            HierarchicalExpansion = hierarchicalExpansion;

            // Note: we're not setting the Controller here, because we want to clone the current one when doing a playout.
            PlayoutBot = new RandomBot();

            // We'll be cutting off the simulations after X turns, using a GoalStrategy.
            Goal = new GoalStrategyTurnCutoff(PLAYOUT_TURN_CUTOFF);

            // Expansion, Application and Goal will be handled by the GameLogic.
            GameLogic = new SabberStoneGameLogic(HierarchicalExpansion, Goal);

            // Simulation will be handled by the Playout.
            Playout = new PlayoutStrategySabberStone(PlayoutBot);

            // Create the INodeEvaluation strategy used in the selection phase.
            var nodeEvaluation = new ScoreUCB<SabberStoneState, SabberStoneAction>(UCT_C_CONSTANT_DEFAULT);

            // Build MCTS
            Builder = MCTS<object, SabberStoneState, SabberStoneAction, object, SabberStoneAction>.Builder();
            Builder.ExpansionStrategy = new MinimumTExpansion<object, SabberStoneState, SabberStoneAction, object, SabberStoneAction>(MIN_T_VISIT_THRESHOLD_FOR_EXPANSION);
            Builder.SelectionStrategy = new BestNodeSelection<object, SabberStoneState, SabberStoneAction, object, SabberStoneAction>(SELECTION_VISIT_MINIMUM_FOR_EVALUATION, nodeEvaluation);
            Builder.EvaluationStrategy = new EvaluationStrategyHearthStone();
            Builder.Iterations = MCTS_NUMBER_OF_ITERATIONS;
            Builder.BackPropagationStrategy = new EvaluateOnceAndColorBackPropagation<object, SabberStoneState, SabberStoneAction, object, SabberStoneAction>();
            Builder.FinalNodeSelectionStrategy = new BestRatioFinalNodeSelection<object, SabberStoneState, SabberStoneAction, object, SabberStoneAction>();
            Builder.SolutionStrategy = new SolutionStrategySabberStone(HierarchicalExpansion);
            Builder.PlayoutStrategy = Playout;
        }

        #endregion

        #region Public Methods

        #region ISabberStoneBot

        /// <summary>
        /// Requests the bot to return a SabberStoneAction based on the current SabberStoneState.
        /// </summary>
        /// <param name="state">The current game state.</param>
        /// <returns>SabberStoneAction.</returns>
        public SabberStoneAction Act(SabberStoneState state) {
            var timer = System.Diagnostics.Stopwatch.StartNew();

            // Set some variables that we will use
            var stateCopy = (SabberStoneState)state.Copy();
            var opponent = stateCopy.Game.CurrentOpponent;

            // TODO Before starting a search, create a determinisation of the opponnent's cards in hand

            //In case we need to obfuscate the state ourselves:
            //
            //what we want to do is first check if we know of any cards in the opponent's deck/hand/secret-zone (e.g. quests)
            //those should not be replaced by random things
            //create a list of the IDs of those known cards and then obfuscate the state while supplying our list of known cards

            // If we are the starting player, we know the opponent has a Coin
            var knownCards = new List<string>();
            if (stateCopy.Game.FirstPlayer.Id == stateCopy.CurrentPlayer()) {
                knownCards.Add("GAME_005");
            }

            stateCopy.Obfuscate(opponent.Id, knownCards);

            // We can get the play history of our opponent (filter out Coin because it never starts in a deck).
            var opponentHistory = opponent.PlayHistory;
            var playedIds = opponentHistory.Where(i => i.SourceCard.Id != "GAME_005").Select(i => i.SourceCard.Id).ToList();
            // TODO try to use these played cards to determine if anything was revealed so we know about it

            //Once the state is correctly obfuscated:
            //
            //try to predict / select what deck the opponent is playing
            var deckDictionary = Decks.AllDecks();
            var possibleDecks = new List<List<Card>>();
            foreach (var item in deckDictionary) {
                var deckIds = Decks.CardIDs(item.Value);
                // A deck can match if all of the cards we have seen played are present
                if (playedIds.All(i => deckIds.Contains(i))) {
                    possibleDecks.Add(item.Value);
                }
            }

            List<Card> selectedDeck;
            // If one deck matches, assume that is the correct deck
            if (possibleDecks.Count == 1)
                selectedDeck = possibleDecks.First();
            //if we can't,
            //either just assume the opponent is playing a pre-set dummy deck
            //or select one of the possible decks at random
            else selectedDeck = possibleDecks.RandomElementOrDefault();

            // Determinise the opponent's cards.
            stateCopy.Determinise(opponent, knownCards, selectedDeck, _rng);

            Console.WriteLine();
            Console.WriteLine(Name());
            Console.WriteLine("Starting an MCTS-search in turn " + (stateCopy.Game.Turn + 1) / 2);

            // Setup a new search with the current state as source.
            SearchContext<object, SabberStoneState, SabberStoneAction, object, SabberStoneAction> context = GameSearchSetup(GameLogic, null, stateCopy, null, Builder.Build());
            
            // Execute the search
            context.Execute();

            // Check if the search was successful
            if (context.Status != SearchStatus.Success) {
                // TODO in case of search failure: throw exception, or print error.
                return SabberStoneAction.CreateNullMove(state.Game.CurrentPlayer);
            }

            var solution = context.Solution;
            var time = timer.ElapsedMilliseconds;
            Console.WriteLine($"MCTS returned with solution: {solution}");
            Console.WriteLine($"My action calculation time was: {time} ms.");
            Console.WriteLine();

            // Check if the solution is a complete action.
            if (solution.IsComplete()) return solution;
            // Otherwise add an End-Turn task before returning.
            Console.WriteLine("Solution was an incomplete action; adding End-Turn task.");
            solution.Tasks.Add(EndTurnTask.Any(Player));
            return solution;
        }

        /// <summary>
        /// Returns the bot's name.
        /// </summary>
        /// <returns>String representing the bot's name.</returns>
        public string Name() {
            return _botName;
        }

        /// <summary>
        /// Returns the player's ID.
        /// </summary>
        /// <returns>Integer representing the player's ID.</returns>
        public int PlayerID() {
            return Player.Id;
        }

        /// <summary>
        /// Sets the Controller that the bot represents within a SabberStone Game.
        /// </summary>
        /// <param name="controller">This bot's Controller.</param>
        public void SetController(Controller controller) {
            Player = controller;
        }

        #endregion

        #endregion

    }
}
