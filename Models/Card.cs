using MTCG.Controller;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace MTCG.Models
{
    public enum CardElement
    {
        Regular,
        Water,
        Fire,
    }

    public enum CardType
    {
        Goblin,
        Troll,
        Elf,
        Knight,
        Dragon,
        Ork,
        Kranken,
        Spell,
    }

    internal class Card
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public properties                                                                                         //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        public Guid Id { get; set; }

        public string Name { get; set; }

        public float Damage { get; set; }


        public void CreateCards(HttpSvrEventArgs e)
        {
            try
            {

                List<Card> package = JsonSerializer.Deserialize<List<Card>>(e.Payload);

                var connectionString = "Host=localhost;Username=swe1user;Password=swe1pw;Database=swe1db";
                using var dataSource = NpgsqlDataSource.Create(connectionString);

                if (package.Count() != 5)
                {
                    e.Reply(400, "Error occured while creating package.");
                    return;
                }

                Guid[] packageids = new Guid[5];
                int i = 0;

                foreach(Card card in package)
                {
                    // Insert data
                    using (var cmd = dataSource.CreateCommand("INSERT INTO cards (id, name, damage, deck, trade, username) VALUES ((@p1), (@p2), (@p3), (@p4), (@p5), (@p6))"))
                    {
                        cmd.Parameters.AddWithValue("@p1", card.Id);
                        cmd.Parameters.AddWithValue("@p2", card.Name);
                        cmd.Parameters.AddWithValue("@p3", card.Damage);
                        cmd.Parameters.AddWithValue("@p4", false);
                        cmd.Parameters.AddWithValue("@p5", false);
                        cmd.Parameters.AddWithValue("@p6", DBNull.Value);
                        cmd.ExecuteNonQuery();
                    }
                    packageids[i] = card.Id;
                    i++;
                }
                using (var cmd = dataSource.CreateCommand("INSERT INTO packages (card1, card2, card3, card4, card5) VALUES ((@p1), (@p2), (@p3), (@p4), (@p5))"))
                {
                    cmd.Parameters.AddWithValue("@p1", packageids[0]);
                    cmd.Parameters.AddWithValue("@p2", packageids[1]);
                    cmd.Parameters.AddWithValue("@p3", packageids[2]);
                    cmd.Parameters.AddWithValue("@p4", packageids[3]);
                    cmd.Parameters.AddWithValue("@p5", packageids[4]);
                    cmd.ExecuteNonQuery();
                }

                    e.Reply(200, "Package created successfully");
            }
            catch (NpgsqlException ex)
            {
                if (ex.SqlState == "23505")
                {
                    e.Reply(400, "Error: Card Uuid already exists.");
                }
                else
                {
                    e.Reply(400, "Error occured while creating package: " + ex.Message);
                }
            }
            catch (Exception ex)
            {
                e.Reply(400, "Error occured while creating package: " + ex.Message);
            }
        }
        
        public Card GetCard(Guid cardid)
        {
            Card card = null;

            try
            {
                card = new Card();

                return card;
            }
            catch
            {

            }

            return card;
        }

        public void GetCards(HttpSvrEventArgs e, UserToken userToken)
        {
            try { 
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
