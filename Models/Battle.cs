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




    internal class Battle
    {
        private UserToken PlayerOne;
        private UserToken PlayerTwo;
        private List<Card> DeckOne;
        private List<Card> DeckTwo;
        private string BattleLog;
        private bool playerOneWins = false;


        public Battle(UserToken playerOne, UserToken playerTwo)
        {
            PlayerOne = playerOne;
            PlayerTwo = playerTwo;
        }
        public string Start()
        {
            CardCollection cardCollection = new CardCollection();
            DeckOne = cardCollection.PrepareDeck(PlayerOne);
            DeckTwo = cardCollection.PrepareDeck(PlayerTwo);
            BattleLog += PlayerOne.LoggedInUser + " is fighting " + PlayerTwo.LoggedInUser + ":\n";

            for (int rounds = 1; rounds <= 100; rounds++)
            {
                if(DeckOne.Count() == 0 || DeckTwo.Count() == 0)
                {
                    break;
                    //end fight
                }
                //randomize an index for both decks
                Random rnd = new Random();
                int indexOne = rnd.Next(DeckOne.Count());
                int indexTwo = rnd.Next(DeckTwo.Count());

                float[] calcDamage = CalculateDamage(DeckOne[indexOne], DeckTwo[indexTwo]);

                BattleLog += "\nRound [" + rounds + "]\n" + PlayerOne.LoggedInUser + ": " + DeckOne[indexOne].Name + " (" + DeckOne[indexOne].Damage + ") vs " +
                    PlayerTwo.LoggedInUser + ": " + DeckTwo[indexTwo].Name + " (" + DeckTwo[indexTwo].Damage + ") => ";

                //special case
                string specialText = SpecialInteraction(DeckOne[indexOne], DeckTwo[indexTwo]);
                if (specialText != ""){
                    BattleLog += specialText;
                }
                else { 
                    BattleLog += DeckOne[indexOne].Damage + " vs " + DeckTwo[indexTwo].Damage + " -> " +
                        calcDamage[0] + " vs " + calcDamage[1] + " => \n";

                    if (calcDamage[0] == calcDamage[1])
                    {
                        BattleLog += "Both cards tie.\n";
                        continue;
                    }
                    else if (calcDamage[0] > calcDamage[1])
                    {
                        playerOneWins = true;
                        BattleLog += DeckOne[indexOne].Name + " wins\n";
                    }
                    else
                    {
                        playerOneWins = false;
                        BattleLog += DeckTwo[indexTwo].Name + " wins\n";
                    }
                }

                ChangeCardAfterRound(indexOne, indexTwo);

            }

            return BattleLog;
        }


        private string SpecialInteraction(Card cardOne, Card cardTwo)
        {
            string winningText = "";
            if(cardOne.Type == CardType.Goblin && cardTwo.Type == CardType.Dragon)
            {
                playerOneWins = false;
                winningText = "Dragon defeats Goblin\n";
                return winningText;
            }
            else if (cardOne.Type == CardType.Dragon && cardTwo.Type == CardType.Goblin)
            {
                playerOneWins = true;
                winningText = "Dragon defeats Goblin\n";
                return winningText;
            }
            else if (cardOne.Type == CardType.Wizzard && cardTwo.Type == CardType.Ork)
            {
                playerOneWins = true;
                winningText = "Wizzard defeats Ork\n";
                return winningText;
            }
            else if (cardOne.Type == CardType.Ork && cardTwo.Type == CardType.Wizzard)
            {
                playerOneWins = false;
                winningText = "Wizzard defeats Ork\n";
                return winningText;
            }
            else if (cardOne.Type == CardType.Knight && (cardTwo.Type == CardType.Spell && cardTwo.Element == CardElement.Water))
            {
                playerOneWins = false;
                winningText = "WaterSpell drowns Knight\n";
                return winningText;
            }
            else if ((cardOne.Type == CardType.Spell && cardOne.Element == CardElement.Water) && cardTwo.Type == CardType.Knight)
            {
                playerOneWins = true;
                winningText = "WaterSpell drowns Knight\n";
                return winningText;
            }
            else if (cardOne.Type == CardType.Kraken && cardTwo.Type == CardType.Spell)
            {
                playerOneWins = true;
                winningText = "Kraken defeats Spell\n";
                return winningText;
            }
            else if (cardOne.Type == CardType.Spell && cardTwo.Type == CardType.Kraken)
            {
                playerOneWins = false;
                winningText = "Kraken defeats Spell\n";
                return winningText;
            }

            return winningText;
        }

        //winner takes over the card of the loser
        private void ChangeCardAfterRound(int indexOne, int indexTwo)
        {
            if (playerOneWins)
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

        //calculates the damage of a round based on element
        private float[] CalculateDamage(Card cardOne, Card cardTwo)
        {
            //float array to return (if necessary) altered damage types
            float[] damage = new float[2];
            cardOne = cardOne.GetCardStats(cardOne);
            cardTwo = cardTwo.GetCardStats(cardTwo);
            //both monsters
            if((cardOne.Type != CardType.Spell) && (cardTwo.Type != CardType.Spell)){
                damage[0] = cardOne.Damage;
                damage[1] = cardTwo.Damage;
            }
            // water -> fire | fire -> regular | regular -> water -- double/halve dmg
            else
            {
                if(cardOne.Element == CardElement.Water && cardTwo.Element == CardElement.Fire)
                {
                    damage[0] = cardOne.Damage*2;
                    damage[1] = cardTwo.Damage/2;
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

        //TODO: Check special
    }
}
