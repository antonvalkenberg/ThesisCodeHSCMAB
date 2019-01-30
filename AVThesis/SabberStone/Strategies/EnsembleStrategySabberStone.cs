using System;
using System.Collections.Generic;
using System.Linq;
using AVThesis.Datastructures;
using AVThesis.Search;
using SabberStoneCore.Model;

/// <summary>
/// Written by A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by G.J. Roelofs and T. Aliyev.
/// </summary>
namespace AVThesis.SabberStone.Strategies {

    /// <summary>
    /// Strategy to run an Ensemble-search for SabberStone.
    /// </summary>
    public class EnsembleStrategySabberStone : IEnsembleStrategy<object, SabberStoneState, SabberStoneAction, object, SabberStoneAction> {

        #region Properties

        /// <summary>
        /// Whether or not to replace cards in player's hand and/or deck with hidden cards.
        /// </summary>
        public bool EnableStateObfuscation { get; set; }

        /// <summary>
        /// Whether or not to allow perfect information (i.e. play with open hands and face-up decks).
        /// </summary>
        public bool EnablePerfectInformation { get; set; }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates a new instance of the EnsembleStrategy for SabberStone.
        /// </summary>
        /// <param name="enableStateObfuscation">Whether or not to allow obfuscation of the game state.</param>
        /// <param name="enablePerfectInformation">Whether or not to allow for perfect information.</param>
        public EnsembleStrategySabberStone(bool enableStateObfuscation, bool enablePerfectInformation) {
            EnableStateObfuscation = enableStateObfuscation;
            EnablePerfectInformation = enablePerfectInformation;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Creates an ensemble of searches based on the provided search function.
        /// </summary>
        /// <param name="context">The current search context.</param>
        /// <param name="searchFunction">The function that runs the actual search.</param>
        /// <param name="ensembleSize">The amount of searches to perform in the ensemble.</param>
        public void EnsembleSearch(SearchContext<object, SabberStoneState, SabberStoneAction, object, SabberStoneAction> context, Func<SearchContext<object, SabberStoneState, SabberStoneAction, object, SabberStoneAction>, SabberStoneState, SabberStoneAction> searchFunction, int ensembleSize) {
            var gameState = context.Source;
            var rootStartedPlaying = gameState.Game.FirstPlayer.Id == gameState.CurrentPlayer();

            // What we want to do is first check if we know of any cards in the opponent's deck/hand/secret-zone (e.g. quests)
            // Those should not be replaced by random things
            // Create a list of the IDs of those known cards and then obfuscate the state while supplying our list of known cards
            // TODO Before obfuscation: try to use the played cards to determine if anything was revealed (which means we know about it and shouldn't hide it)

            // Determine known cards for root player
            var knownRootCards = new List<string>();
            // Cards currently in hand
            knownRootCards.AddRange(gameState.Game.CurrentPlayer.HandZone.Select(i => i.Card.Id));
            // Cards that have been played
            var playedRootCards = gameState.Game.CurrentPlayer.PlayHistory.Select(i => i.SourceCard.Id).ToList();

            // Determine known cards for opponent
            var knownOpponentCards = new List<string>();
            // If we are the starting player, we know the opponent has a Coin
            if (rootStartedPlaying) {
                knownOpponentCards.Add("GAME_005");
            }
            // We can get the play history of our opponent (filter out Coin because it never starts in a deck)
            var playedOpponentCards = gameState.Game.CurrentOpponent.PlayHistory.Where(i => i.SourceCard.Id != "GAME_005").Select(i => i.SourceCard.Id).ToList();

            // Obfuscate the state
            if (EnableStateObfuscation && !EnablePerfectInformation) {
                // For root player
                gameState.Obfuscate(gameState.Game.CurrentPlayer.Id, knownRootCards);
                // For opponent
                gameState.Obfuscate(gameState.Game.CurrentOpponent.Id, knownOpponentCards);
            }

            for (var i = 0; i < ensembleSize; i++) {
                // Clone the context
                var clonedContext = context.Copy();

                if (!EnablePerfectInformation) {
                    // Try to predict / select what deck the opponent is playing
                    var deckDictionary = Decks.AllDecks();
                    var possibleDecks = new List<List<Card>>();
                    foreach (var item in deckDictionary) {
                        var deckIds = Decks.CardIDs(item.Value);
                        // A deck can match if all of the cards we have seen played are present
                        if (playedOpponentCards.All(j => deckIds.Contains(j))) {
                            possibleDecks.Add(item.Value);
                        }
                    }

                    // If only one deck matches, assume that is the correct deck
                    // If more decks are possible matches, select one of those possible decks at random
                    var selectedDeck = possibleDecks.Count == 1 ? possibleDecks.First() : possibleDecks.RandomElementOrDefault();

                    // Determinise the game state for the root player's cards-in-deck and for the opponent's cards-in-hand and cards-in-deck
                    var determinedRootCards = new List<string>(knownRootCards).Concat(playedRootCards).ToList();
                    var determinedOpponentCards = new List<string>(knownOpponentCards).Concat(playedOpponentCards).ToList();
                    clonedContext.Source.Determinise(determinedRootCards, determinedOpponentCards, selectedDeck);
                }

                // Call the search function
                var solution = searchFunction(clonedContext, context.Source);

                // Use domain in SearchContext to save solutions / task statistics
                //TODO Ensemble -> save solution somewhere
            }

            throw new NotImplementedException();
        }

        public SabberStoneAction Solution(SearchContext<object, SabberStoneState, SabberStoneAction, object, SabberStoneAction> context) {
            throw new NotImplementedException();
        }

        #endregion

    }

}
