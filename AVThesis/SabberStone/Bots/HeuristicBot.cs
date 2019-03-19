using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using SabberStoneCore.Enums;
using SabberStoneCore.Exceptions;
using SabberStoneCore.Model;
using SabberStoneCore.Model.Entities;
using SabberStoneCore.Model.Zones;
using SabberStoneCore.Tasks;
using SabberStoneCore.Tasks.PlayerTasks;

/*
 * EVA.cs
 * 
 * Copyright (c) 2018, Pablo Garcia-Sanchez. All rights reserved.
 *
 * This library is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 2.1 of the License, or (at your option) any later version.
 *
 * This library is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public
 * License along with this library; if not, write to the Free Software
 * Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston,
 * MA 02110-1301  USA
 * 
 * Contributors:
 * Alberto Tonda (INRA)
 */
/// <summary>
/// Written by A.J.J. Valkenberg, used in his Master Thesis on Artificial Intelligence.
/// In parts inspired by a code framework written by G.J. Roelofs and T. Aliyev.
/// </summary>
namespace AVThesis.SabberStone.Bots {

    /// <summary>
    /// A bot that chooses its moves based on a heuristic scoring of available moves, choosing the best score.
    /// This is an adaptation of the code submitted by Pablo Garcia-Sanchez and Alberto Tonda to the Hearthstone AI Competition at CIG 2018.
    /// </summary>
    public class HeuristicBot : ISabberStoneBot {

        #region POGame

        partial class POGame {
            private SabberStoneCore.Model.Game game;
            private SabberStoneCore.Model.Game origGame;
            private bool debug;

            public POGame(SabberStoneCore.Model.Game game, bool debug) {
                this.origGame = game;
                this.game = game.Clone();
                game.Player1.Game = game;
                game.Player2.Game = game;
                prepareOpponent();
                this.debug = debug;

                if (debug) {
                    Console.WriteLine("Game Board");
                    Console.WriteLine(game.FullPrint());
                }
            }

            private void prepareOpponent() {
                int nr_deck_cards = game.CurrentOpponent.DeckZone.Count;
                int nr_hand_cards = game.CurrentOpponent.HandZone.Count;

                game.CurrentOpponent.DeckCards = Decks.DebugDeck;

                //DebugCardsGen.AddAll(game.CurrentOpponent.DeckCards);
                game.CurrentOpponent.HandZone = new HandZone(game.CurrentOpponent);
                game.CurrentOpponent.DeckZone = new DeckZone(game.CurrentOpponent);

                for (int i = 0; i < nr_hand_cards; i++) {
                    addCardToZone(game.CurrentOpponent.HandZone, game.CurrentOpponent.DeckCards[i], game.CurrentOpponent);
                }

                for (int i = 0; i < nr_deck_cards; i++) {
                    addCardToZone(game.CurrentOpponent.DeckZone, game.CurrentOpponent.DeckCards[nr_hand_cards + i], game.CurrentOpponent);
                }
            }

            private void addCardToZone(IZone zone, Card card, Controller player) {
                var tags = new Dictionary<GameTag, int>();
                tags[GameTag.ENTITY_ID] = game.NextId;
                tags[GameTag.CONTROLLER] = player.PlayerId;
                tags[GameTag.ZONE] = (int)zone.Type;
                IPlayable playable = null;


                switch (card.Type) {
                    case CardType.MINION:
                        playable = new Minion(player, card, tags);
                        break;

                    case CardType.SPELL:
                        playable = new Spell(player, card, tags);
                        break;

                    case CardType.WEAPON:
                        playable = new Weapon(player, card, tags);
                        break;

                    case CardType.HERO:
                        tags[GameTag.ZONE] = (int)SabberStoneCore.Enums.Zone.PLAY;
                        tags[GameTag.CARDTYPE] = card[GameTag.CARDTYPE];
                        playable = new Hero(player, card, tags);
                        break;

                    case CardType.HERO_POWER:
                        tags[GameTag.COST] = card[GameTag.COST];
                        tags[GameTag.ZONE] = (int)SabberStoneCore.Enums.Zone.PLAY;
                        tags[GameTag.CARDTYPE] = card[GameTag.CARDTYPE];
                        playable = new HeroPower(player, card, tags);
                        break;

                    default:
                        throw new EntityException($"Couldn't create entity, because of an unknown cardType {card.Type}.");
                }

                zone?.Add(playable);
            }

            private void CreateFullInformationGame(List<Card> deck_player1, DeckZone deckzone_player1, HandZone handzone_player1, List<Card> deck_player2, DeckZone deckzone_player2, HandZone handzone_player2) {
                game.Player1.DeckCards = deck_player1;
                game.Player1.DeckZone = deckzone_player1;
                game.Player1.HandZone = handzone_player1;

                game.Player2.DeckCards = deck_player2;
                game.Player2.DeckZone = deckzone_player2;
                game.Player2.HandZone = handzone_player2;
            }

            public void Process(PlayerTask task) {
                game.Process(task);
            }

            /**
             * Simulates the tasks against the current game and
             * returns a Dictionary with the following POGame-Object
             * for each task (or null if an exception happened
             * during that game)
             */
            public Dictionary<PlayerTask, POGame> Simulate(List<PlayerTask> tasksToSimulate) {
                Dictionary<PlayerTask, POGame> simulated = new Dictionary<PlayerTask, POGame>();
                foreach (PlayerTask task in tasksToSimulate) {
                    SabberStoneCore.Model.Game clone = this.origGame.Clone();
                    try {
                        clone.Process(task);
                        simulated.Add(task, new POGame(clone, this.debug));
                    }
                    catch (Exception) {
                        simulated.Add(task, null);
                    }
                }
                return simulated;

            }

            public POGame getCopy(bool? debug = null) {
                return new POGame(origGame, debug ?? this.debug);
            }


            public string FullPrint() {
                return game.FullPrint();
            }

            public string PartialPrint() {
                var str = new StringBuilder();
                if (game.CurrentPlayer == game.Player1) {
                    str.AppendLine(game.Player1.HandZone.FullPrint());
                    str.AppendLine(game.Player1.Hero.FullPrint());
                    str.AppendLine(game.Player1.BoardZone.FullPrint());
                    str.AppendLine(game.Player2.BoardZone.FullPrint());
                    str.AppendLine(game.Player2.Hero.FullPrint());
                    str.AppendLine($"Opponent Hand Cards: {game.Player2.HandZone.Count}");
                }
                if (game.CurrentPlayer == game.Player2) {
                    str.AppendLine($"Opponent Hand Cards: {game.Player1.HandZone.Count}");
                    str.AppendLine(game.Player1.Hero.FullPrint());
                    str.AppendLine(game.Player1.BoardZone.FullPrint());
                    str.AppendLine(game.Player2.BoardZone.FullPrint());
                    str.AppendLine(game.Player2.Hero.FullPrint());
                    str.AppendLine(game.Player2.HandZone.FullPrint());
                }

                return str.ToString();
            }


        }

        /// <summary>
        /// Standard Getters for the current game
        /// </summary>
        partial class POGame {

            /// <summary>
            /// Gets or sets the turn count.
            /// </summary>
            /// <value>The amount of player turns that happened in the game. When the game starts (after Mulligan),
            /// value will equal 1.</value>
            public int Turn {
                get { return game[GameTag.TURN]; }
            }

            /// <summary>
            /// Gets or sets the game state.
            /// </summary>
            /// <value><see cref="State"/></value>
            public State State {
                get { return (State)game[GameTag.STATE]; }
            }

            /// <summary>
            /// Gets or sets the first card played this turn.
            /// </summary>
            /// <value>The entityID of the card.</value>
            public int FirstCardPlayedThisTurn {
                get { return game[GameTag.FIRST_CARD_PLAYED_THIS_TURN]; }
            }

            /// <summary>
            /// The controller which goes 'first'. This player's turn starts after Mulligan.
            /// </summary>
            /// <value><see cref="Controller"/></value>
            public Controller FirstPlayer {
                get {
                    return game.Player1[GameTag.FIRST_PLAYER] == 1 ? game.Player1 : game.Player2[GameTag.FIRST_PLAYER] == 1 ? game.Player2 : null;
                }
            }

            /// <summary>
            /// Gets or sets the controller delegating the current turn.
            /// </summary>
            /// <value><see cref="Controller"/></value>
            public Controller CurrentPlayer => game.CurrentPlayer;

            /// <summary>
            /// Gets the opponent controller of <see cref="CurrentPlayer"/>.
            /// </summary>
            /// <value><see cref="Controller"/></value>
            public Controller CurrentOpponent => game.CurrentOpponent;

            /// <summary>
            /// Gets or sets the CURRENT step. These steps occur within <see cref="State.RUNNING"/> and
            /// indicate states which are used to process actions.
            /// </summary>
            /// <value><see cref="Step"/></value>
            public Step Step {
                //get { return (Step)this[GameTag.STEP]; }
                //set { this[GameTag.STEP] = (int)value; }
                get { return (Step)game.GetNativeGameTag(GameTag.STEP); }
            }

            /// <summary>
            /// Gets or sets the NEXT step. <seealso cref="Step"/>
            /// </summary>
            /// <value><see cref="Step"/></value>
            public Step NextStep {
                get { return (Step)game.GetNativeGameTag(GameTag.NEXT_STEP); }
            }

            /// <summary>
            /// Gets or sets the number of killed minions for this turn.
            /// </summary>
            /// <value>The amount of killed minions.</value>
            public int NumMinionsKilledThisTurn {
                get { return game[GameTag.NUM_MINIONS_KILLED_THIS_TURN]; }
            }

            /// <summary>Gets the heroes.</summary>
            /// <value><see cref="Hero"/></value>
            public List<Hero> Heroes => game.Heroes;

            /// <summary>Gets ALL minions (from both sides of the board).</summary>
            /// <value><see cref="Minion"/></value>
            public List<Minion> Minions => game.Minions;

            /// <summary>Gets ALL characters.</summary>
            /// <value><see cref="ICharacter"/></value>
            public List<ICharacter> Characters => game.Characters;
        }

        #endregion

        #region Fields

        private const string BOT_NAME = "HeuristicBot";
        public static bool _debug = false;

        public static int NUM_PARAMETERS = 21;
        public static string HERO_HEALTH_REDUCED = "HERO_HEALTH_REDUCED";
        public static string HERO_ATTACK_REDUCED = "HERO_ATTACK_REDUCED";
        public static string MINION_HEALTH_REDUCED = "MINION_HEALTH_REDUCED";
        public static string MINION_ATTACK_REDUCED = "MINION_ATTACK_REDUCED";
        public static string MINION_KILLED = "MINION_KILLED";
        public static string MINION_APPEARED = "MINION_APPEARED";
        public static string SECRET_REMOVED = "SECRET_REMOVED";
        public static string MANA_REDUCED = "MANA_REDUCED";

        public static string M_HEALTH = "M_HEALTH";
        public static string M_ATTACK = "M_ATTACK";
        //public static string M_HAS_BATTLECRY = "M_HAS_BATTLECRY";
        public static string M_HAS_CHARGE = "M_HAS_CHARGE";
        public static string M_HAS_DEAHTRATTLE = "M_HAS_DEAHTRATTLE";
        public static string M_HAS_DIVINE_SHIELD = "M_HAS_DIVINE_SHIELD";
        public static string M_HAS_INSPIRE = "M_HAS_INSPIRE";
        public static string M_HAS_LIFE_STEAL = "M_HAS_LIFE_STEAL";
        public static string M_HAS_STEALTH = "M_HAS_STEALTH";
        public static string M_HAS_TAUNT = "M_HAS_TAUNT";
        public static string M_HAS_WINDFURY = "M_HAS_WINDFURY";
        public static string M_RARITY = "M_RARITY";
        public static string M_MANA_COST = "M_MANA_COST";
        public static string M_POISONOUS = "M_POISONOUS";

        public Dictionary<string, double> weights;

        #endregion

        #region Properties

        /// <summary>
        /// The player this bot is representing in a game of SabberStone.
        /// </summary>
        public Controller Player { get; set; }

        #endregion

        #region Constructors

        public HeuristicBot() {
            debug("Initializing EVA");
            this.setAgentWeightsFromString("0.569460712743#0.958111820041#0.0689492467097#0.0#0.843573987219#0.700225423688#0.907680353441#0.0#0.993682660717#1.0#0.640753949511#0.992872512338#0.92870036875#0.168100484322#0.870080107454#0.0#0.42897762808#1.0#0.0#0.583884736646#0.0");
        }

        #endregion

        #region Private Methods

        private PlayerTask GetMove(POGame poGame) {

            debug("CURRENT TURN: " + poGame.Turn);
            KeyValuePair<PlayerTask, double> p = getBestTask(poGame);
            debug("SELECTED TASK TO EXECUTE " + stringTask(p.Key) + "HAS A SCORE OF " + p.Value);

            debug("-------------------------------------");
            //Console.ReadKey();

            return p.Key;
        }

        //Mejor hacer esto con todas las posibles en cada movimiento
        private double scoreTask(POGame before, POGame after) {
            if (after == null) { //There was an exception with the simullation function!
                return 1; //better than END_TURN, just in case
            }

            if (after.CurrentOpponent.Hero.Health <= 0) {
                debug("KILLING ENEMY!!!!!!!!");
                return Int32.MaxValue;
            }
            if (after.CurrentPlayer.Hero.Health <= 0) {
                debug("WARNING: KILLING MYSELF!!!!!");
                return Int32.MinValue;
            }


            //Differences in Health
            debug("CALCULATING ENEMY HEALTH SCORE");
            double enemyPoints = calculateScoreHero(before.CurrentOpponent, after.CurrentOpponent);
            debug("CALCULATING MY HEALTH SCORE");
            double myPoints = calculateScoreHero(before.CurrentPlayer, after.CurrentPlayer);
            debug("Enemy points: " + enemyPoints + " My points: " + myPoints);

            //Differences in Minions
            debug("CALCULATING ENEMY MINIONS");
            double scoreEnemyMinions = calculateScoreMinions(before.CurrentOpponent.BoardZone, after.CurrentOpponent.BoardZone);
            debug("Score enemy minions: " + scoreEnemyMinions);
            debug("CALCULATING MY MINIONS");
            double scoreMyMinions = calculateScoreMinions(before.CurrentPlayer.BoardZone, after.CurrentPlayer.BoardZone);
            debug("Score my minions: " + scoreMyMinions);

            //Differences in Secrets
            debug("CALCULATING SECRETS");
            double scoreEnemySecrets = calculateScoreSecretsRemoved(before.CurrentOpponent, after.CurrentOpponent);
            double scoreMySecrets = calculateScoreSecretsRemoved(before.CurrentPlayer, after.CurrentPlayer);


            //Difference in Mana
            int usedMana = before.CurrentPlayer.RemainingMana - after.CurrentPlayer.RemainingMana;
            double scoreManaUsed = usedMana * weights[MANA_REDUCED];
            debug("Final task score" + enemyPoints + ",neg(" + myPoints + ")," + scoreEnemyMinions + ",neg(" + scoreMyMinions + "),S:" + scoreEnemySecrets + "neg( " + scoreMySecrets + ") M:neg(:" + scoreManaUsed + ")");
            return enemyPoints - myPoints + scoreEnemyMinions - scoreMyMinions + scoreEnemySecrets - scoreMySecrets - scoreManaUsed;
        }

        private double calculateScoreHero(Controller playerBefore, Controller playerAfter) {
            debug(playerBefore.Hero.Health + "(" + playerBefore.Hero.Armor + ")/" + playerBefore.Hero.AttackDamage + " --> " +
                 playerAfter.Hero.Health + "(" + playerAfter.Hero.Armor + ")/" + playerAfter.Hero.AttackDamage
                );
            int diffHealth = (playerBefore.Hero.Health + playerBefore.Hero.Armor) - (playerAfter.Hero.Health + playerAfter.Hero.Armor);
            int diffAttack = (playerBefore.Hero.AttackDamage) - (playerAfter.Hero.AttackDamage);
            //debug("DIFS"+diffHealth + " " + diffAttack);
            double score = diffHealth * weights[HERO_HEALTH_REDUCED] + diffAttack * weights[HERO_ATTACK_REDUCED];
            return score;
        }

        private double calculateScoreMinions(BoardZone before, BoardZone after) {
            foreach (Minion m in before.GetAll()) {
                debug("BEFORE " + stringMinion(m));
            }

            foreach (Minion m in after.GetAll()) {
                debug("AFTER  " + stringMinion(m));
            }


            double scoreHealthReduced = 0;
            double scoreAttackReduced = 0; //We should add Divine shield removed?
            double scoreKilled = 0;
            double scoreAppeared = 0;

            //Minions modified?
            foreach (Minion mb in before.GetAll()) {
                bool survived = false;
                foreach (Minion ma in after.GetAll()) {
                    if (ma.Id == mb.Id) {
                        scoreHealthReduced = scoreHealthReduced + weights[MINION_HEALTH_REDUCED] * (mb.Health - ma.Health) * scoreMinion(mb); //Positive points if health is reduced
                        scoreAttackReduced = scoreAttackReduced + weights[MINION_ATTACK_REDUCED] * (mb.AttackDamage - ma.AttackDamage) * scoreMinion(mb); //Positive points if attack is reduced
                        survived = true;

                    }
                }

                if (survived == false) {
                    debug(stringMinion(mb) + " was killed");
                    scoreKilled = scoreKilled + scoreMinion(mb) * weights[MINION_KILLED]; //WHATEVER //Positive points if card is dead
                }

            }

            //New Minions on play?
            foreach (Minion ma in after.GetAll()) {
                bool existed = false;
                foreach (Minion mb in before.GetAll()) {
                    if (ma.Id == mb.Id) {
                        existed = true;
                    }
                }
                if (existed == false) {
                    debug(stringMinion(ma) + " is NEW!!");
                    scoreAppeared = scoreAppeared + scoreMinion(ma) * weights[MINION_APPEARED]; //Negative if a minion appeared (below)
                }
            }

            //Think always as positive points if the enemy suffers!
            return scoreHealthReduced + scoreAttackReduced + scoreKilled - scoreAppeared; //CHANGE THESE SIGNS ACCORDINGLY!!!

        }

        private double calculateScoreSecretsRemoved(Controller playerBefore, Controller playerAfter) {

            int dif = playerBefore.SecretZone.Count - playerAfter.SecretZone.Count;
            /*if (dif != 0) {
				Console.WriteLine("STOP");
			}*/
            //int dif = playerBefore.NumSecretsPlayedThisGame - playerAfter.NumSecretsPlayedThisGame;
            return dif * weights[SECRET_REMOVED];
        }

        private double scoreMinion(Minion m) {
            //return 1;

            double score = m.Health * weights[M_HEALTH] + m.AttackDamage * weights[M_ATTACK];
            /*if (m.HasBattleCry)
				score = score + weights[M_HAS_BATTLECRY];*/
            if (m.HasCharge)
                score = score + weights[M_HAS_CHARGE];
            if (m.HasDeathrattle)
                score = score + weights[M_HAS_DEAHTRATTLE];
            if (m.HasDivineShield)
                score = score + weights[M_HAS_DIVINE_SHIELD];
            if (m.HasInspire)
                score = score + weights[M_HAS_INSPIRE];
            if (m.HasLifeSteal)
                score = score + weights[M_HAS_LIFE_STEAL];
            if (m.HasTaunt)
                score = score + weights[M_HAS_TAUNT];
            if (m.HasWindfury)
                score = score + weights[M_HAS_WINDFURY];



            score = score + m.Card.Cost * weights[M_MANA_COST];
            score = score + rarityToInt(m.Card) * weights[M_RARITY];
            if (m.Poisonous) {
                score = score + weights[M_POISONOUS];
            }
            return score;

        }

        private int rarityToInt(Card c) {
            if (c.Rarity == Rarity.COMMON) {
                return 1;
            }
            if (c.Rarity == Rarity.FREE) {
                return 1;
            }
            if (c.Rarity == Rarity.RARE) {
                return 2;
            }
            if (c.Rarity == Rarity.EPIC) {
                return 3;
            }
            if (c.Rarity == Rarity.LEGENDARY) {
                return 4;
            }
            return 0;
        }

        private KeyValuePair<PlayerTask, double> getBestTask(POGame state) {
            double bestScore = Double.MinValue;
            PlayerTask bestTask = null;
            List<PlayerTask> list = state.CurrentPlayer.Options();
            foreach (PlayerTask t in list) {
                debug("---->POSSIBLE " + stringTask(t));

                double score = 0;
                POGame before = state;
                if (t.PlayerTaskType == PlayerTaskType.END_TURN) {
                    score = 0;
                }
                else {
                    List<PlayerTask> toSimulate = new List<PlayerTask>();
                    toSimulate.Add(t);
                    Dictionary<PlayerTask, POGame> simulated = state.Simulate(toSimulate);
                    //Console.WriteLine("SIMULATION COMPLETE");
                    POGame nextState = simulated[t];
                    score = scoreTask(state, nextState); //Warning: if using tree, avoid overflow with max values!


                }
                debug("SCORE " + score);
                if (score >= bestScore) {
                    bestTask = t;
                    bestScore = score;
                }

            }

            return new KeyValuePair<PlayerTask, double>(bestTask, bestScore);
        }

        private void setAgentWeights(double[] w) { //EVA has this function private!
            this.weights = new Dictionary<string, double>();
            this.weights.Add(HERO_HEALTH_REDUCED, w[0]);
            this.weights.Add(HERO_ATTACK_REDUCED, w[1]);
            this.weights.Add(MINION_HEALTH_REDUCED, w[2]);
            this.weights.Add(MINION_ATTACK_REDUCED, w[3]);
            this.weights.Add(MINION_APPEARED, w[4]);
            this.weights.Add(MINION_KILLED, w[5]);
            this.weights.Add(SECRET_REMOVED, w[6]);
            this.weights.Add(MANA_REDUCED, w[7]);
            this.weights.Add(M_HEALTH, w[8]);
            this.weights.Add(M_ATTACK, w[9]);
            this.weights.Add(M_HAS_CHARGE, w[10]);
            this.weights.Add(M_HAS_DEAHTRATTLE, w[11]);
            this.weights.Add(M_HAS_DIVINE_SHIELD, w[12]);
            this.weights.Add(M_HAS_INSPIRE, w[13]);
            this.weights.Add(M_HAS_LIFE_STEAL, w[14]);
            this.weights.Add(M_HAS_STEALTH, w[15]);
            this.weights.Add(M_HAS_TAUNT, w[16]);
            this.weights.Add(M_HAS_WINDFURY, w[17]);
            this.weights.Add(M_RARITY, w[18]);
            this.weights.Add(M_MANA_COST, w[19]);
            this.weights.Add(M_POISONOUS, w[20]);

        }

        private void setAgentWeightsFromString(string weights) { //This function is private, just in case 
            debug("Setting agent weights from string");
            string[] vs = weights.Split('#');

            if (vs.Length != NUM_PARAMETERS)
                throw new Exception("NUM VALUES NOT CORRECT");

            double[] ws = new double[NUM_PARAMETERS];
            for (int i = 0; i < ws.Length; i++) {
                ws[i] = Double.Parse(vs[i], CultureInfo.InvariantCulture);
            }

            this.setAgentWeights(ws);
        }

        private string stringTask(PlayerTask task) {
            string t = "TASK: " + task.PlayerTaskType + " " + task.Source + "----->" + task.Target;
            if (task.Target != null)
                t = t + task.Target.Controller.PlayerId;
            else
                t = t + "No target";
            return t;
        }

        private string stringMinion(Minion m) {
            return m + " " + m.AttackDamage + "/" + m.Health;
        }

        private void debug(string line) {
            if (_debug)
                Console.WriteLine(line);
        }

        #endregion

        #region Public Methods

        public SabberStoneAction Act(SabberStoneState state) {
            var timer = System.Diagnostics.Stopwatch.StartNew();
            var gameState = (SabberStoneState)state.Copy();
            
            if (_debug) Console.WriteLine();
            if (_debug) Console.WriteLine(Name());
            if (_debug) Console.WriteLine($"Starting a Heuristic search in turn {(gameState.Game.Turn + 1) / 2}");

            var solution = new SabberStoneAction();

            while (gameState.CurrentPlayer() == PlayerID() && gameState.Game.State != State.COMPLETE) {
                var poGame = new POGame(gameState.Game, _debug);
                var task = GetMove(poGame);
                solution.AddTask((SabberStonePlayerTask)task);
                gameState.Game.Process(task);
            }

            var time = timer.ElapsedMilliseconds;
            if (_debug) Console.WriteLine();
            if (_debug) Console.WriteLine($"Heuristic returned with solution: {solution}");
            if (_debug) Console.WriteLine($"My total calculation time was: {time} ms.");

            // Check if the solution is a complete action.
            if (!solution.IsComplete()) {
                // Otherwise add an End-Turn task before returning.
                if (_debug) Console.WriteLine("Solution was an incomplete action; adding End-Turn task.");
                solution.Tasks.Add((SabberStonePlayerTask)EndTurnTask.Any(Player));
            }

            if (_debug) Console.WriteLine();
            return solution;
        }

        public void SetController(Controller controller) {
            Player = controller;
        }

        public int PlayerID() {
            return Player.Id;
        }

        public string Name() {
            return BOT_NAME;
        }

        public long BudgetSpent() {
            return 0;
        }

        #endregion

    }

}
