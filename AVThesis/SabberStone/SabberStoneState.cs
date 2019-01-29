﻿using System;
using System.Collections.Generic;
using System.Linq;
using AVThesis.Search;
using SabberStoneCore.Model;
using SabberStoneCore.Model.Entities;

/// <summary>
/// Written by A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by G.J. Roelofs and T. Aliyev.
/// </summary>
namespace AVThesis.SabberStone {

    /// <summary>
    /// Wrapper class for using a SabberStone Game as a State in the search framework.
    /// </summary>
    public class SabberStoneState : State {

        #region Properties

        /// <summary>
        /// The SabberStone Game.
        /// </summary>
        public SabberStoneCore.Model.Game Game { get; set; }

        /// <summary>
        /// The 1st player in the SabberStone Game.
        /// </summary>
        public Controller Player1 => Game.Player1;

        /// <summary>
        /// The 2nd player in the SabberStone Game.
        /// </summary>
        public Controller Player2 => Game.Player2;

        /// <summary>
        /// The ID of the player that has won. Note: defaults to <see cref="State.DRAW"/>.
        /// </summary>
        public new int PlayerWon
        {
            get
            {
                if (Game.State != SabberStoneCore.Enums.State.COMPLETE) return DRAW;
                return Player1.PlayState == SabberStoneCore.Enums.PlayState.WON ? Player1.Id : Player2.Id;
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructs a new instance of a SabberStoneState.
        /// </summary>
        /// <param name="game">The SabberStone Game that the new SabberStoneState should represents.</param>
        public SabberStoneState(SabberStoneCore.Model.Game game) {
            Game = game;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Obfuscates this state by hiding the cards in the HandZone, DeckZone and SecretZone of the specified player.
        /// </summary>
        /// <param name="playerID">The unique identifier of the player who's information should be obfuscated.</param>
        public void Obfuscate(int playerID) {
            Obfuscate(playerID, new List<string>());
        }

        /// <summary>
        /// Obfuscates this state by hiding the cards in the HandZone, DeckZone and SecretZone of the specified player.
        /// </summary>
        /// <param name="playerID">The unique identifier of the player who's information should be obfuscated.</param>
        /// <param name="knownCards">Collection of cards known to the opposing player. These cards will not be obfuscated.</param>
        public void Obfuscate(int playerID, List<string> knownCards) {
            // Make a copy of the knownCards, so we don't alter it
            var knownCardsCopy = new List<string>(knownCards);

            Controller obfuscatePlayer;
            if (playerID == Player1.Id) {
                obfuscatePlayer = Player1;
            }
            else if (playerID == Player2.Id) {
                obfuscatePlayer = Player2;
            }
            else return;

            // Hand
            var removeItems = new List<IPlayable>();
            for (int i = 0; i < obfuscatePlayer.HandZone.Count; i++) {
                if (!knownCardsCopy.Contains(obfuscatePlayer.HandZone[i].Card.Id)) {
                    removeItems.Add(obfuscatePlayer.HandZone[i]);
                }
                else {
                    knownCardsCopy.Remove(obfuscatePlayer.HandZone[i].Card.Id);
                }
            }
            foreach (var item in removeItems) {
                obfuscatePlayer.HandZone.Remove(item);
                obfuscatePlayer.HandZone.Add(Entity.FromCard(obfuscatePlayer, Util.HiddenCard));
            }

            // Deck
            removeItems = new List<IPlayable>();
            for (int i = 0; i < obfuscatePlayer.DeckZone.Count; i++) {
                if (!knownCardsCopy.Contains(obfuscatePlayer.DeckZone[i].Card.Id)) {
                    removeItems.Add(obfuscatePlayer.DeckZone[i]);
                }
                else {
                    knownCardsCopy.Remove(obfuscatePlayer.DeckZone[i].Card.Id);
                }
            }
            foreach (var item in removeItems) {
                obfuscatePlayer.DeckZone.Remove(item);
                obfuscatePlayer.DeckZone.Add(Entity.FromCard(obfuscatePlayer, Util.HiddenCard));
            }

            // Secrets
            removeItems = new List<IPlayable>();
            for (int i = 0; i < obfuscatePlayer.SecretZone.Count; i++) {
                if (!knownCardsCopy.Contains(obfuscatePlayer.SecretZone[i].Card.Id)) {
                    removeItems.Add(obfuscatePlayer.SecretZone[i]);
                }
                else {
                    knownCardsCopy.Remove(obfuscatePlayer.SecretZone[i].Card.Id);
                }
            }
            foreach (var item in removeItems) {
                obfuscatePlayer.SecretZone.Remove(item);
                obfuscatePlayer.SecretZone.Add(new Spell(obfuscatePlayer, Util.HiddenCard, Util.HiddenCard.Tags));
            }

        }

        /// <summary>
        /// Determinise the unknown cards in this state,  while leaving any known cards in place.
        /// </summary>
        /// <param name="knownRootPlayerCards">The known cards in the root player's hand and the cards that player has played this game.</param>
        /// <param name="knownOpponentCards">The known cards in the opponent's hand and the cards that player has played this game.</param>
        /// <param name="selectedDeck">The deck that had been selected to represent the unknown contents of the opponent's deck.</param>
        public void Determinise(List<string> knownRootPlayerCards, List<string> knownOpponentCards, List<Card> selectedDeck) {
            
            #region Root Player
            var rootPlayerDeck = Game.CurrentPlayer.DeckCards;

            // TODO Determinisation -> re-write method to also consider root player

            #endregion

            #region Opponent
            var opponent = Game.CurrentOpponent;

            // Remove any known cards from the opponent's deck, those will already be in their correct place
            var knownCardsCopy = new List<string>(knownOpponentCards);
            foreach (var item in selectedDeck) {
                if (!knownCardsCopy.Contains(item.Id)) continue;
                knownCardsCopy.Remove(item.Id);
                selectedDeck.Remove(item);
            }

            // Select an amount of cards from the deck that will replace the hidden-cards in the opponent's hand.
            // TODO There can be several strategies to do determinisation (random, best-case, worst-case)
            var opponentCards = opponent.HandZone.GetAll();
            foreach (var item in opponentCards) {
                // Don't replace if we know a card is supposed to be there.
                if (!knownOpponentCards.Contains(item.Card.Id)) {
                    opponent.HandZone.Remove(item);
                    var randomPosition = Util.RNG.Next(selectedDeck.Count);
                    var randomDeckCard = selectedDeck.ElementAt(randomPosition);
                    selectedDeck.RemoveAt(randomPosition);
                    opponent.HandZone.Add(Entity.FromCard(opponent, randomDeckCard));
                }
                else {
                    // Remove the card from the known cards list, so we replace any other copies that we do not know about.
                    knownOpponentCards.Remove(item.Card.Id);
                }
            }

            // The opponent's deck will now become whatever cards are left from the deck.
            // This can be randomised, or a random card can be selected whenever a card is drawn from the deck.
            var opponentDeck = opponent.DeckZone.GetAll();
            // Small check to see if the sizes are compatible.
            if (opponentDeck.Count() > selectedDeck.Count())
                Console.WriteLine("WARNING: More cards in opponent's deck than in selected deck.");
            foreach (var item in opponentDeck) {
                // Don't replace if we know a card is supposed to be there.
                if (!knownOpponentCards.Contains(item.Card.Id)) {
                    opponent.DeckZone.Remove(item);
                    var randomPosition = Util.RNG.Next(selectedDeck.Count);
                    var randomDeckCard = selectedDeck.ElementAt(randomPosition);
                    selectedDeck.RemoveAt(randomPosition);
                    opponent.DeckZone.Add(Entity.FromCard(opponent, randomDeckCard));
                }
                else {
                    // Remove the card from the known cards list, so we replace any other copies that we do not know about.
                    knownOpponentCards.Remove(item.Card.Id);
                }
            }

            #endregion

        }

        #endregion

        #region Overridden Methods

        /// <summary>
        /// Copies this SabberStoneState and returns it.
        /// </summary>
        /// <returns>New SabberStoneState that is a copy of this SabberStoneState.</returns>
        public override dynamic Copy() {
            return new SabberStoneState(Game.Clone());
        }

        /// <summary>
        /// Returns the unique identifier of the currently active player.
        /// </summary>
        /// <returns>The unique identifier of the currently active player.</returns>
        public override int CurrentPlayer() {
            return Game.CurrentPlayer.Id;
        }

        /// <summary>
        /// Whether or not this SabberStoneState is equal to the argument SabberStoneState.
        /// </summary>
        /// <param name="otherState">The SabberStoneState to check with this SabberStoneState for equality.</param>
        /// <returns>Boolean indicating whether or not the argument SabberStoneState is equal to this SabberStoneState.
        public override bool Equals(State otherState) {
            return HashMethod() == otherState.HashMethod();
        }

        /// <summary>
        /// Returns the SabberStoneState's hash code.
        /// </summary>
        /// <returns>The SabberStoneState's hash code.</returns>
        public override long HashMethod() {
            return Game.Hash().GetHashCode();
        }

        /// <summary>
        /// Returns the number of players in the game.
        /// </summary>
        /// <returns>The number of players in the game.</returns>
        public override int NumberOfPlayers() {
            return 2;
        }

        #endregion

    }
}
