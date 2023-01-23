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
        public string? Type { get; set; }
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
            catch
            {
                e.Reply(400, "Error occured while fetching trade deals.");
            }
        }

        // saves a trading deal in the db
        public void PostTradingDeal(HttpSvrEventArgs e, UserToken userToken)
        {

            try
            {
                Trade? newTrade = JsonSerializer.Deserialize<Trade>(e.Payload);

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
            catch
            {
                e.Reply(400, "Error occured while creating trade.");
            }
        }

        // deletes a trading deal in the db
        public void DeleteTradingDeal(HttpSvrEventArgs e, UserToken userToken)
        {
            try
            {
                string[] pathId = e.Path.Split("/");

                Trade? Trade = new Trade();
                Trade.Id = Guid.Parse(pathId[2]);

                var connectionString = "Host=localhost;Username=swe1user;Password=swe1pw;Database=swe1db";
                using var dataSource = NpgsqlDataSource.Create(connectionString);
                int deletedRows = 0;

                using (var cmd = dataSource.CreateCommand("DELETE FROM trade WHERE (@p1) IN (SELECT tradeid FROM trade JOIN cards ON trade.cardtotrade = cards.id WHERE cards.username = (@p2))"))
                {
                    cmd.Parameters.AddWithValue("@p1", Trade.Id);
                    cmd.Parameters.AddWithValue("@p2", userToken.LoggedInUser);

                    deletedRows = cmd.ExecuteNonQuery();
                }

                if(deletedRows == 0)
                {
                    e.Reply(400, "Error occured while deleting trade.");
                    return;
                }

                e.Reply(200, "Trade deal deleted successfully.");
            }
            catch
            {
                e.Reply(400, "Error occured while deleting trade.");
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

                Trade? Trade = new Trade();
                Trade.Id = Guid.Parse(pathId[2]);
                string usernameTrade = "";

                // get trading deal with username
                using (var cmd = dataSource.CreateCommand("SELECT cards.username, trade.type, trade.mindmg, trade.cardtotrade FROM trade JOIN cards ON trade.cardtotrade = cards.id WHERE trade.tradeid = (@p1)"))
                {
                    cmd.Parameters.AddWithValue("@p1", Trade.Id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        cmd.Parameters.AddWithValue("@p1", Trade.Id);
                        while (reader.Read())
                        {
                            usernameTrade = reader.GetString(0);
                            Trade.Type = reader.GetString(1);
                            Trade.MinimumDamage = (float)reader.GetDouble(2);
                            Trade.CardToTrade = reader.GetGuid(3);
                        }
                    }
                }

                if (usernameTrade.Equals(userToken.LoggedInUser))
                {
                    e.Reply(400, "Can't trade with yourself");
                    return;
                }

                Card biddingCard = new Card();
                //parsing payload AND replacing masked quotes
                biddingCard.Id = Guid.Parse(e.Payload.Replace("\"", ""));

                //get card to trade stats
                using (var cmd = dataSource.CreateCommand("SELECT name, damage FROM cards WHERE cards.id = (@p1)"))
                {
                    cmd.Parameters.AddWithValue("@p1", biddingCard.Id);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            biddingCard.Name = reader.GetString(0);
                            biddingCard.Damage = (float)reader.GetDouble(1);
                        }
                    }
                }
                //check if the requirements are met
                biddingCard = biddingCard.GetCardStats(biddingCard);
                bool match = false;

                if(biddingCard.Type == Card.CardType.Spell && Trade.Type.Equals("spell"))
                {
                    match = true;
                }
                else if (biddingCard.Type != Card.CardType.Spell && Trade.Type.Equals("monster"))
                {
                    match = true;
                }
                if ((biddingCard.Damage < Trade.MinimumDamage) && match) 
                {
                    match = false;
                }

                if (!match) 
                {
                    e.Reply(400, "Minimum requirements not met");
                    return;
                }
                //process trade transaction
                using (var cmd = dataSource.CreateCommand("UPDATE cards SET username = (@p1) WHERE id = (@p2)"))
                {
                    cmd.Parameters.AddWithValue("@p1", userToken.LoggedInUser);
                    cmd.Parameters.AddWithValue("@p2", Trade.CardToTrade);
                    cmd.ExecuteNonQuery();
                }
                using (var cmd = dataSource.CreateCommand("UPDATE cards SET username = (@p1) WHERE id = (@p2)"))
                {
                    cmd.Parameters.AddWithValue("@p1", usernameTrade);
                    cmd.Parameters.AddWithValue("@p2", biddingCard.Id);
                    cmd.ExecuteNonQuery();
                }

                e.Reply(200, "Trade was succesfull!");
            }
            catch
            {
                e.Reply(400, "Error occured while trading.");
            }
        }
    }
}
