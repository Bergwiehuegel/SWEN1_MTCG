using MTCG.Controller;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Models
{
    internal class Battle
    {

        public string Start(UserToken playerOne, UserToken playerTwo)
        {
            CardCollection cardCollection = new CardCollection();
            List<Card> deckOne = cardCollection.PrepareDeck(playerOne);
            List<Card> deckTwo = cardCollection.PrepareDeck(playerTwo);

            for (int rounds = 1; rounds <= 100; rounds++)
            {

            }
            Console.WriteLine("Joined: " + playerOne + " " + playerTwo);
            Console.WriteLine("Thread ID:" + Thread.CurrentThread.ManagedThreadId);
            Thread.Sleep(100);
            return "TestLog";
        }

        
    }
}
