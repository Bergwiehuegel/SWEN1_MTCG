using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using System.Xml.Linq;
using System.Text.Json;
using MTCG.Controller;
using Npgsql;
using System.Drawing;
using System.Security.Cryptography;
using Microsoft.VisualBasic;

namespace MTCG.Models
{
    internal class CardCollection
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // private properties                                                                                        //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////


        public void PrintDeck(HttpSvrEventArgs e, UserToken userToken)
        {
            try
            {
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
                if (replyString != "Your Deck: \n")
                {
                    e.Reply(200, replyString);
                }
                else
                {
                    e.Reply(200, "No cards in your deck yet.");
                }
            }
            catch (Exception ex)
            {
                e.Reply(400, "Error occured while fetching cards: " + ex.Message);
            }
        }

        public void UpdateDeck(HttpSvrEventArgs e, UserToken userToken)
        {
            try
            {
                Guid[] deck = JsonSerializer.Deserialize<Guid[]>(e.Payload);
                if (deck.Length != 4)
                {
                    e.Reply(400, "Malformed Request to update Decks.");
                    return;
                }

                var connectionString = "Host=localhost;Username=swe1user;Password=swe1pw;Database=swe1db";
                using var dataSource = NpgsqlDataSource.Create(connectionString);

                // Checks if requested cards are valid for deckbuilding (belong to the user and are not flagged for trading)
                int requestedCardNumber = 0;
                using (var cmd = dataSource.CreateCommand("SELECT count(*) from cards WHERE (username = (@p1) AND trade = false AND (id = (@p2) OR id = (@p3) OR id = (@p4) OR id = (@p5)))"))
                {
                    cmd.Parameters.AddWithValue("@p1", userToken.LoggedInUser);
                    cmd.Parameters.AddWithValue("@p2", deck[0]);
                    cmd.Parameters.AddWithValue("@p3", deck[1]);
                    cmd.Parameters.AddWithValue("@p4", deck[2]);
                    cmd.Parameters.AddWithValue("@p5", deck[3]);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            requestedCardNumber = reader.GetInt32(0);
                        }
                    }
                }
                if (requestedCardNumber < 4)
                {
                    e.Reply(400, "Not all requested Cards are available or in your collection.");
                    return;
                }
                Console.WriteLine("AFTER CHECK INT = " + requestedCardNumber);
                //Update current deck flags to false
                using (var cmd = dataSource.CreateCommand("UPDATE cards SET deck = false WHERE (username = (@p1) AND deck = true)"))
                {
                    cmd.Parameters.AddWithValue("@p1", userToken.LoggedInUser);
                    cmd.ExecuteNonQuery();
                }
                Console.WriteLine("AFTER UPDATE OLD");
                //Update new deck flags
                using (var cmd = dataSource.CreateCommand("UPDATE cards SET deck = true WHERE (username = (@p1) AND (id = (@p2) OR id = (@p3) OR id = (@p4) OR id = (@p5)))"))
                {
                    cmd.Parameters.AddWithValue("@p1", userToken.LoggedInUser);
                    cmd.Parameters.AddWithValue("@p2", deck[0]);
                    cmd.Parameters.AddWithValue("@p3", deck[1]);
                    cmd.Parameters.AddWithValue("@p4", deck[2]);
                    cmd.Parameters.AddWithValue("@p5", deck[3]);
                    cmd.ExecuteNonQuery();
                }
                Console.WriteLine("AFTER UPDATE NEW");

                e.Reply(200, "Deck updated successfully.");
            }
            catch (Exception ex)
            {
                e.Reply(400, "Error occured while fetching cards: " + ex.Message);
            }
        }

        public List<Card> PrepareDeck(UserToken userToken)
        {
            List<Card>? FullDeck = new List<Card>();
            try
            {
                var connectionString = "Host=localhost;Username=swe1user;Password=swe1pw;Database=swe1db";
                using var dataSource = NpgsqlDataSource.Create(connectionString);

                // Retrieve all cards with the deck tag belonging to the user
                using (var cmd = dataSource.CreateCommand("SELECT id, name, damage FROM cards WHERE username = (@p1) AND deck = TRUE"))
                {
                    cmd.Parameters.AddWithValue("@p1", userToken.LoggedInUser);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Card newCard = new Card();
                            newCard.Id = reader.GetGuid(0);
                            newCard.Name = reader.GetString(1);
                            newCard.Damage = (float) reader.GetDouble(2);
                            FullDeck.Add(newCard);
                        }
                    }
                }


                return FullDeck;
            }
            catch
            {
                throw;
            }
        }
        public void GetCards(HttpSvrEventArgs e, UserToken userToken)
        {
            try
            {
                var connectionString = "Host=localhost;Username=swe1user;Password=swe1pw;Database=swe1db";
                using var dataSource = NpgsqlDataSource.Create(connectionString);

                // Retrieve all cards belonging to the user
                string replyString = "Your Cards: \n";
                using (var cmd = dataSource.CreateCommand("SELECT name, damage FROM cards WHERE username = (@p1)"))
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
                if (replyString != "Your Cards: \n")
                {
                    e.Reply(200, replyString);
                }
                else
                {
                    e.Reply(200, "No cards in your Collection.");
                }

            }
            catch (Exception ex)
            {
                e.Reply(400, "Error occured while fetching cards: " + ex.Message);
            }
        }
    }
}
