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
        /// All decks indexed by name.
        /// </summary>
        /// <returns>Dictionary containing all decks indexed by name.</returns>
        public static Dictionary<string, List<Card>> AllDecks() {
            return new Dictionary<string, List<Card>>() {
                { "DefaultDeck", new List<Card>(DefaultDeck) },
                { "AggroHunter", new List<Card>(AggroHunter) },
                { "MidrangeHunter", new List<Card>(MidrangeHunter) },
                { "ControlHunter", new List<Card>(ControlHunter) },
            };
        }

        /// <summary>
        /// A default deck containing simple cards.
        /// </summary>
        public static List<Card> DefaultDeck => new List<Card>() {
            Cards.FromName("Arcane Shot"),
            Cards.FromName("Arcane Shot"),
            Cards.FromName("Goldshire Footman"),
            Cards.FromName("Goldshire Footman"),
            Cards.FromName("Murloc Raider"),
            Cards.FromName("Murloc Raider"),
            Cards.FromName("Bloodfen Raptor"),
            Cards.FromName("Frostwolf Grunt"),
            Cards.FromName("Frostwolf Grunt"),
            Cards.FromName("River Crocolisk"),
            Cards.FromName("Ironfur Grizzly"),
            Cards.FromName("Ironfur Grizzly"),
            Cards.FromName("Razorfen Hunter"),
            Cards.FromName("Shattered Sun Cleric"),
            Cards.FromName("Chillwind Yeti"),
            Cards.FromName("Chillwind Yeti"),
            Cards.FromName("Sen'jin Shieldmasta"),
            Cards.FromName("Sen'jin Shieldmasta"),
            Cards.FromName("Booty Bay Bodyguard"),
            Cards.FromName("Booty Bay Bodyguard"),
            Cards.FromName("Nightblade"),
            Cards.FromName("Nightblade"),
            Cards.FromName("Boulderfist Ogre"),
            Cards.FromName("Boulderfist Ogre"),
            Cards.FromName("Lord of the Arena"),
            Cards.FromName("Lord of the Arena"),
            Cards.FromName("Core Hound"),
            Cards.FromName("Core Hound"),
            Cards.FromName("Stormwind Champion"),
            Cards.FromName("Stormwind Champion")
        };

        /// <summary>
        /// An aggressive version of a Hunter deck.
        /// </summary>
        public static List<Card> AggroHunter => new List<Card>() {
            Cards.FromName("Abusive Sergeant"),
            Cards.FromName("Abusive Sergeant"),
            Cards.FromName("Acherus Veteran"),
            Cards.FromName("Acherus Veteran"),
            Cards.FromName("Arcane Shot"),
            Cards.FromName("Arcane Shot"),
            Cards.FromName("Brave Archer"),
            Cards.FromName("Brave Archer"),
            Cards.FromName("Emerald Hive Queen"),
            Cards.FromName("Emerald Hive Queen"),
            Cards.FromName("Emerald Reaver"),
            Cards.FromName("Emerald Reaver"),
            Cards.FromName("Fiery Bat"),
            Cards.FromName("Fiery Bat"),
            Cards.FromName("Timber Wolf"),
            Cards.FromName("Timber Wolf"),
            Cards.FromName("Dire Wolf Alpha"),
            Cards.FromName("Dire Wolf Alpha"),
            Cards.FromName("Duskboar"),
            Cards.FromName("Duskboar"),
            Cards.FromName("Fallen Sun Cleric"),
            Cards.FromName("Fallen Sun Cleric"),
            Cards.FromName("Kindly Grandmother"),
            Cards.FromName("Kindly Grandmother"),
            Cards.FromName("Quick Shot"),
            Cards.FromName("Quick Shot"),
            Cards.FromName("Bearshark"),
            Cards.FromName("Bearshark"),
            Cards.FromName("Kill Command"),
            Cards.FromName("Kill Command")
        };

        /// <summary>
        /// A midrange version of a Hunter deck.
        /// </summary>
        public static List<Card> MidrangeHunter => new List<Card>() {
            Cards.FromName("Dire Mole"),
            Cards.FromName("Dire Mole"),
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
            Cards.FromName("Bearshark"),
            Cards.FromName("Bearshark"),
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
        /// A controlling version of a Hunter deck.
        /// </summary>
        public static List<Card> ControlHunter => new List<Card>() {
            Cards.FromName("Arcane Shot"),
            Cards.FromName("Arcane Shot"),
            Cards.FromName("Candleshot"),
            Cards.FromName("Candleshot"),
            Cards.FromName("On the Hunt"),
            Cards.FromName("On the Hunt"),
            Cards.FromName("Hunter's Mark"),
            Cards.FromName("Hunter's Mark"),
            Cards.FromName("Quick Shot"),
            Cards.FromName("Quick Shot"),
            Cards.FromName("Carrion Grub"),
            Cards.FromName("Carrion Grub"),
            Cards.FromName("Deadly Shot"),
            Cards.FromName("Fungal Enchanter"),
            Cards.FromName("Fungal Enchanter"),
            Cards.FromName("Flanking Strike"),
            Cards.FromName("Flanking Strike"),
            Cards.FromName("Lifedrinker"),
            Cards.FromName("Lifedrinker"),
            Cards.FromName("Wing Blast"),
            Cards.FromName("Wing Blast"),
            Cards.FromName("Antique Healbot"),
            Cards.FromName("Cobra Shot"),
            Cards.FromName("Cobra Shot"),
            Cards.FromName("Rotten Applebaum"),
            Cards.FromName("Vilebrood Skitterer"),
            Cards.FromName("Maexxna"),
            Cards.FromName("Savannah Highmane"),
            Cards.FromName("Savannah Highmane"),
            Cards.FromName("King Krush")
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
