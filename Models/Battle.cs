using MTCG.Controller;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MTCG.Models.Card;

namespace MTCG.Models
{




    public class Battle
    {
        private UserToken PlayerOne { get; set; }
        private UserToken PlayerTwo { get; set; }
        public List<Card> DeckOne { get; set; } = new List<Card>();
        public List<Card> DeckTwo { get; set; } = new List<Card>();
        private string BattleLog { get; set; }
        public bool PlayerOneWins { get; set; } = false;
        public bool IsSuddenDeathMatch = false;


        //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // constructors                                                                                             //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public Battle(UserToken playerOne, UserToken playerTwo)
        {
            PlayerOne = playerOne;
            PlayerTwo = playerTwo;
        }

        // starts a battle for 2 players with max 100 rounds
        // TODO: Sudden Death Match! (if players tie after 100 rounds they enter an extra 10 rounds where cards get taken out of the deck non permanently)
        public string Start()
        {
            try
            {
                CardCollection cardCollection = new CardCollection();
                DeckOne = cardCollection.PrepareDeck(PlayerOne);
                DeckTwo = cardCollection.PrepareDeck(PlayerTwo);
                BattleLog += PlayerOne.LoggedInUser + " is fighting " + PlayerTwo.LoggedInUser + ":\n";

                for (int rounds = 1; rounds <= 100; rounds++)
                {
                    if (DeckOne.Count() == 0)
                    {
                        BattleLog += "\n" + PlayerTwo.LoggedInUser + " wins the battle! Congratulations!\n";
                        UpdateStats(true, PlayerTwo);
                        UpdateStats(false, PlayerOne);
                        break;
                    }
                    if (DeckTwo.Count() == 0)
                    {
                        BattleLog += "\n" + PlayerOne.LoggedInUser + " wins the battle! Congratulations!\n";
                        UpdateStats(false, PlayerTwo);
                        UpdateStats(true, PlayerOne);
                        break;
                    }
                    if(IsSuddenDeathMatch)
                    {
                        rounds += 9;
                    }
                    //randomize an index for both decks
                    Random rnd = new Random();
                    int indexOne = rnd.Next(DeckOne.Count());
                    int indexTwo = rnd.Next(DeckTwo.Count());

                    float[] calcDamage = CalculateDamage(DeckOne[indexOne], DeckTwo[indexTwo]);

                    BattleLog += "\nRound [" + (IsSuddenDeathMatch ? rounds/10 : rounds) + "]\n" + PlayerOne.LoggedInUser + ": " + DeckOne[indexOne].Name + " (" + DeckOne[indexOne].Damage + ") vs " +
                        PlayerTwo.LoggedInUser + ": " + DeckTwo[indexTwo].Name + " (" + DeckTwo[indexTwo].Damage + ") => ";

                    //special case
                    string specialText = SpecialInteraction(DeckOne[indexOne], DeckTwo[indexTwo]);
                    if (specialText != "")
                    {
                        BattleLog += specialText;
                    }
                    else
                    {
                        BattleLog += DeckOne[indexOne].Damage + " vs " + DeckTwo[indexTwo].Damage + " -> " +
                            calcDamage[0] + " vs " + calcDamage[1] + " => \n";

                        if (calcDamage[0] == calcDamage[1])
                        {
                            BattleLog += "Both cards tie.\n";
                            if (rounds == 100)
                            {
                                if (!IsSuddenDeathMatch)
                                {
                                    IsSuddenDeathMatch = true;
                                    rounds = 1;
                                    BattleLog += "INITIATING SUDDEN DEATHMATCH! (Cards get now removed - not taken over and a winning card gets a boost do dmg)";
                                    continue;
                                }
                                else
                                {
                                    break;
                                }
                            }
                            continue;
                        }
                        else if (calcDamage[0] > calcDamage[1])
                        {
                            PlayerOneWins = true;
                            BattleLog += DeckOne[indexOne].Name + " wins\n";
                        }
                        else
                        {
                            PlayerOneWins = false;
                            BattleLog += DeckTwo[indexTwo].Name + " wins\n";
                        }
                    }
                    if (!IsSuddenDeathMatch)
                    {
                        ChangeCardAfterRound(indexOne, indexTwo);
                    }
                    else
                    {
                        RemoveCardAfterRoundAndBoostDmg(indexOne, indexTwo);
                    }

                    if (rounds == 100)
                    {
                        BattleLog += "\nBattle ended in a tie!\n";
                        if (!IsSuddenDeathMatch)
                        {
                            IsSuddenDeathMatch = true;
                            rounds = 0;
                            BattleLog += "\nINITIATING SUDDEN DEATHMATCH! (Cards now get a damage boost if they win a round and losing cards get removed)\n";
                        }
                    }
                }

                return BattleLog;
            }
            catch
            {
                throw;
            }
        }

        // checks for special card interactions that immediately win the round
        public string SpecialInteraction(Card cardOne, Card cardTwo)
        {
            try
            {
                string winningText = "";
                if (cardOne.Type == CardType.Goblin && cardTwo.Type == CardType.Dragon)
                {
                    PlayerOneWins = false;
                    winningText = "Dragon defeats Goblin\n";
                    return winningText;
                }
                else if (cardOne.Type == CardType.Dragon && cardTwo.Type == CardType.Goblin)
                {
                    PlayerOneWins = true;
                    winningText = "Dragon defeats Goblin\n";
                    return winningText;
                }
                else if (cardOne.Type == CardType.Wizzard && cardTwo.Type == CardType.Ork)
                {
                    PlayerOneWins = true;
                    winningText = "Wizzard defeats Ork\n";
                    return winningText;
                }
                else if (cardOne.Type == CardType.Ork && cardTwo.Type == CardType.Wizzard)
                {
                    PlayerOneWins = false;
                    winningText = "Wizzard defeats Ork\n";
                    return winningText;
                }
                else if (cardOne.Type == CardType.Knight && (cardTwo.Type == CardType.Spell && cardTwo.Element == CardElement.Water))
                {
                    PlayerOneWins = false;
                    winningText = "WaterSpell drowns Knight\n";
                    return winningText;
                }
                else if ((cardOne.Type == CardType.Spell && cardOne.Element == CardElement.Water) && cardTwo.Type == CardType.Knight)
                {
                    PlayerOneWins = true;
                    winningText = "WaterSpell drowns Knight\n";
                    return winningText;
                }
                else if (cardOne.Type == CardType.Kraken && cardTwo.Type == CardType.Spell)
                {
                    PlayerOneWins = true;
                    winningText = "Kraken defeats Spell\n";
                    return winningText;
                }
                else if (cardOne.Type == CardType.Spell && cardTwo.Type == CardType.Kraken)
                {
                    PlayerOneWins = false;
                    winningText = "Kraken defeats Spell\n";
                    return winningText;
                }

                return winningText;
            }
            catch
            {
                throw;
            }
        }

        // winner takes over the card of the loser
        public void ChangeCardAfterRound(int indexOne, int indexTwo)
        {
            try { 
                if (PlayerOneWins)
                {
                    DeckOne.Add(DeckTwo[indexTwo]);
                    DeckTwo.RemoveAt(indexTwo);
                }
                else
                {
                    DeckTwo.Add(DeckOne[indexOne]);
                    DeckOne.RemoveAt(indexOne);
                }
            }
            catch
            {
                throw;
            }
        }

        // looser card gets removed and winning card gets a dmg boost of 11
        public void RemoveCardAfterRoundAndBoostDmg(int indexOne, int indexTwo)
        {
            try
            {
                if (PlayerOneWins)
                {
                    DeckTwo.RemoveAt(indexTwo);
                    DeckOne[indexOne].Damage += 11.0F;
                }
                else
                {
                    DeckOne.RemoveAt(indexOne);
                    DeckTwo[indexTwo].Damage += 11.0F;
                }
            }
            catch
            {
                throw;
            }
        }

        // calculates the damage of a round based on element
        // water -> fire | fire -> regular | regular -> water -- double/halve dmg if [Spell vs Spell] or [Spell vs Monster]
        public float[] CalculateDamage(Card cardOne, Card cardTwo)
        {
            try { 
                //float array to return (if necessary) altered damage types
                float[] damage = new float[2];
                cardOne = cardOne.GetCardStats(cardOne);
                cardTwo = cardTwo.GetCardStats(cardTwo);
                //both monsters
                if ((cardOne.Type != CardType.Spell) && (cardTwo.Type != CardType.Spell)) {
                    damage[0] = cardOne.Damage;
                    damage[1] = cardTwo.Damage;
                }
                else
                {
                    if (cardOne.Element == CardElement.Water && cardTwo.Element == CardElement.Fire)
                    {
                        damage[0] = cardOne.Damage * 2;
                        damage[1] = cardTwo.Damage / 2;
                    }
                    else if (cardOne.Element == CardElement.Fire && cardTwo.Element == CardElement.Regular)
                    {
                        damage[0] = cardOne.Damage * 2;
                        damage[1] = cardTwo.Damage / 2;
                    }
                    else if (cardOne.Element == CardElement.Regular && cardTwo.Element == CardElement.Water)
                    {
                        damage[0] = cardOne.Damage * 2;
                        damage[1] = cardTwo.Damage / 2;
                    }
                    //both the same element - no changes
                    else
                    {
                        damage[0] = cardOne.Damage;
                        damage[1] = cardTwo.Damage;
                    }
                }
                return damage;
            }
            catch
            {
                throw;
            }
        }

        // updates the user stats after a battle
        public void UpdateStats(bool win, UserToken userToken)
        {
            try
            {
                var connectionString = "Host=localhost;Username=swe1user;Password=swe1pw;Database=swe1db";
                using var dataSource = NpgsqlDataSource.Create(connectionString);

                if (win)
                {
                    using (var cmd = dataSource.CreateCommand("UPDATE stats SET wins = wins + 1, elo = elo + 3 WHERE username = (@p1)"))
                    {
                        cmd.Parameters.AddWithValue("@p1", userToken.LoggedInUser);
                        cmd.ExecuteNonQuery();
                    }
                }
                else
                {
                    using (var cmd = dataSource.CreateCommand("UPDATE stats SET losses = losses + 1, elo = elo -3 WHERE username = (@p1)"))
                    {
                        cmd.Parameters.AddWithValue("@p1", userToken.LoggedInUser);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch
            {
                throw;
            }
        }
    }
}
