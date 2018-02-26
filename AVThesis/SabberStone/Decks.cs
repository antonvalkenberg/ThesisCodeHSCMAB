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

        /// <summary>
        /// A deck for testing purposes.
        /// </summary>
        public static List<Card> TestDeck => new List<Card>() {
            Cards.FromName("Arcane Shot"),
            Cards.FromName("Arcane Shot"),
            Cards.FromName("Stonetusk Boar"),
            Cards.FromName("Stonetusk Boar")
        };

    }
}
