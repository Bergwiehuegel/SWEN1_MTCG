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
                //chose a random card deck one
                Random rnd = new Random();
                int indexOne = rnd.Next(DeckOne.Count());
                int indexTwo = rnd.Next(DeckTwo.Count());

                float[] calcDamage = CalculateDamage(DeckOne[indexOne], DeckTwo[indexTwo]);
                BattleLog += PlayerOne.LoggedInUser + ": " + DeckOne[indexOne].Name + " (" + DeckOne[indexOne].Damage + ") vs " +
                    PlayerTwo.LoggedInUser + ": " + DeckTwo[indexTwo].Name + " (" + DeckTwo[indexTwo].Damage + ") => " +
                    DeckOne[indexOne].Damage + " vs " + DeckTwo[indexTwo].Damage + " -> " +
                    calcDamage[0] + " vs " + calcDamage[1] + " => \n";




            }

            return BattleLog;
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
