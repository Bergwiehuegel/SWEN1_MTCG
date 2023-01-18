using MTCG.Server;
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
        private List<Card> DeckOne { get; set; }
        private List<Card> DeckTwo { get; set; }

        public string Start(UserToken playerOne, UserToken playerTwo)
        {
            DeckOne = prepareDeck(playerOne);
            DeckTwo = prepareDeck(playerTwo);

            for (int rounds = 0; rounds < 100; rounds++)
            {

            }
            Console.WriteLine("Joined: " + playerOne + " " + playerTwo);
            Console.WriteLine("Thread ID:" + Thread.CurrentThread.ManagedThreadId);
            Thread.Sleep(1000);
            return "TestLog";
        }

        private List<Card> prepareDeck(UserToken userToken)
        {
            //try
            //{
            List<Card> deck = new List<Card>();
            var connectionString = "Host=localhost;Username=swe1user;Password=swe1pw;Database=swe1db";
            using var dataSource = NpgsqlDataSource.Create(connectionString);

            // Retrieve all cards with the deck tag belonging to the user
            string replyString = "Your Deck: \n";
            using (var cmd = dataSource.CreateCommand("SELECT name, damage FROM cards WHERE username = (@p1) AND deck = TRUE"))
            {
                cmd.Parameters.AddWithValue("@p1", userToken.LoggedInUser);
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        replyString += "Cardname: " + reader.GetString(0) + " - Damage: " + string.Format("{0:0.0}", reader.GetDouble(1)) + "\n";
                    }
                }
            }

            return deck;
            //}
            //catch (Exception ex)
            //{

            //}
        }
    }
}
