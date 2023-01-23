using MTCG.Controller;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MTCG.Models
{
    internal class Trade
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public properties                                                                                         //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public Guid Id { get; set; }
        public Guid CardToTrade { get; set; }
        public string Type { get; set; }
        public float MinimumDamage { get; set; }


        //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public methods                                                                                           //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        // db request for all trading deals available - reply to client
        public void GetTradingDeals(HttpSvrEventArgs e, UserToken userToken)
        {
            try
            {
                var connectionString = "Host=localhost;Username=swe1user;Password=swe1pw;Database=swe1db";
                using var dataSource = NpgsqlDataSource.Create(connectionString);

                // Retrieve all trade deals
                string replyString = "Trade deals available: \n\n";

                using (var cmd = dataSource.CreateCommand("SELECT trade.tradeid, cards.name, cards.damage, trade.type, trade.mindmg FROM trade JOIN cards ON trade.cardtotrade = cards.id"))
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            replyString += "Trade ID: " + reader.GetGuid(0) +
                                "\nCard Offer:\n[Type: " + reader.GetString(1) +
                                " - Damage: " + reader.GetDouble(2) +
                                "]\nWanted:\n[Type: " + reader.GetString(3) +
                                " - Minimum Damage: " + reader.GetDouble(4) + "]\n\n";
                        }
                    }
                }
                e.Reply(200, replyString);
            }
            catch (Exception ex)
            {
                e.Reply(400, "Error occured while fetching trade deals: " + ex.Message);
            }
        }

        // saves a trading deal in the db
        public void PostTradingDeal(HttpSvrEventArgs e, UserToken userToken)
        {

            try
            {
                Trade newTrade = JsonSerializer.Deserialize<Trade>(e.Payload);
              
                var connectionString = "Host=localhost;Username=swe1user;Password=swe1pw;Database=swe1db";
                using var dataSource = NpgsqlDataSource.Create(connectionString);

                //TODO: check if card in possession/available for trade
                using (var cmd = dataSource.CreateCommand("INSERT INTO trade (tradeID, cardToTrade, type, minDmg) VALUES ((@p1), (@p2), (@p3), (@p4))"))
                {
                    cmd.Parameters.AddWithValue("@p1", newTrade.Id);
                    cmd.Parameters.AddWithValue("@p2", newTrade.CardToTrade);
                    cmd.Parameters.AddWithValue("@p3", newTrade.Type);
                    cmd.Parameters.AddWithValue("@p4", newTrade.MinimumDamage);
                    cmd.ExecuteNonQuery();
                }
                e.Reply(200, "Trade deal created successfully.");
            }
            catch (Exception ex)
            {
                e.Reply(400, "Error occured while creating trade: " + ex.Message);
            }
        }

        //db request for trading deal and bidding card - cards are traded if the min. requirements are met
        public void TryTrade(HttpSvrEventArgs e, UserToken userToken)
        {
            try
            {
                string[] pathId = e.Path.Split("/");
                
                var connectionString = "Host=localhost;Username=swe1user;Password=swe1pw;Database=swe1db";
                using var dataSource = NpgsqlDataSource.Create(connectionString);

                // Retrieve all cards belonging to the user
                string replyString = "";
                using (var cmd = dataSource.CreateCommand("SELECT username, coins, name, bio, image FROM users WHERE username = (@p1)"))
                {
                    cmd.Parameters.AddWithValue("@p1", userToken.LoggedInUser);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            replyString += "username: " + reader.GetString(0) +
                                "\ncoins: " + reader.GetInt64(1) +
                                "\nname: " + (reader.IsDBNull(2) ? "" : reader.GetString(2)) +
                                "\nbio: " + (reader.IsDBNull(3) ? "" : reader.GetString(3)) +
                                "\nimage: " + (reader.IsDBNull(4) ? "" : reader.GetString(4)) + "\n";
                        }
                    }
                }
                e.Reply(200, replyString);
            }
            catch (Exception ex)
            {
                e.Reply(400, "Error occured while fetching profile data: " + ex.Message);
            }
            //Get card

            //check type

            //check min dmg
        }
    }
}
