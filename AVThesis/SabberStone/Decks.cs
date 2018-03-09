using System.Collections.Generic;
using SabberStoneCore.Model;

/// <summary>
/// Written by A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by G.J. Roelofs and T. Aliyev.
/// </summary>
namespace AVThesis.SabberStone {

    /// <summary>
    /// Contains pre-made Hearthstone decks.
    /// </summary>
    public class Decks {

        public static Dictionary<string, List<Card>> AllDecks() {
            return new Dictionary<string, List<Card>>() {
                { "TestDeck", new List<Card>(TestDeck) }
            };
        }

        /// <summary>
        /// A deck for testing purposes.
        /// </summary>
        public static List<Card> TestDeck => new List<Card>() {
            Cards.FromName("Alleycat"),
            Cards.FromName("Alleycat"),
            Cards.FromName("Arcane Shot"),
            Cards.FromName("Arcane Shot"),
            Cards.FromName("Stonetusk Boar"),
            Cards.FromName("Stonetusk Boar"),
            Cards.FromName("Timber Wolf"),
            Cards.FromName("Timber Wolf"),
            Cards.FromName("Bloodfen Raptor"),
            Cards.FromName("Bloodfen Raptor"),
            Cards.FromName("Dire Wolf Alpha"),
            Cards.FromName("Dire Wolf Alpha"),
            Cards.FromName("Scavenging Hyena"),
            Cards.FromName("Scavenging Hyena"),
            Cards.FromName("Ironfur Grizzly"),
            Cards.FromName("Ironfur Grizzly"),
            Cards.FromName("Kill Command"),
            Cards.FromName("Kill Command"),
            //Cards.FromName("Unleash the Hounds"),
            //Cards.FromName("Unleash the Hounds"),
            Cards.FromName("Houndmaster"),
            Cards.FromName("Houndmaster"),
            Cards.FromName("Oasis Snapjaw"),
            Cards.FromName("Oasis Snapjaw"),
            Cards.FromName("Starving Buzzard"),
            Cards.FromName("Starving Buzzard"),
            Cards.FromName("Savannah Highmane"),
            Cards.FromName("Savannah Highmane"),
            Cards.FromName("Core Hound"),
            Cards.FromName("Core Hound")
        };

        /// <summary>
        /// Returns the Card identifiers from a deck of Cards.
        /// </summary>
        /// <param name="deck">The deck to get the identifiers from.</param>
        /// <returns>Collection of strings representing the Card identifiers in the deck.</returns>
        public static List<string> CardIDs(List<Card> deck) {
            List<string> cardIds = new List<string>();
            foreach (var item in deck) {
                cardIds.Add(item.Id);
            }
            return cardIds;
        }

    }
}
