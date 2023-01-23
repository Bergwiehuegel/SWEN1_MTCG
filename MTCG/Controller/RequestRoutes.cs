using MTCG.Models;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading;

namespace MTCG.Controller
{
    // utility class with a function that routes based on path in the request
    // needs an authenticated user for most paths
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // public methods                                                                                            //
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////
    public class RequestRoutes
    {
        public static void _Svr_Incoming(object evt)
        {
            HttpSvrEventArgs e = (HttpSvrEventArgs)evt;

            try
            {
                //user token to pass username and check logged in status
                UserToken userToken = new UserToken(); 
                userToken.AuthenticateUser(e);
                User user = new User();

                if (e.Path == "/users" && e.Method== "POST") 
                {
                    user.CreateUser(e);
                }
                else if (e.Path == "/sessions" && e.Method == "POST") 
                {
                    user.LoginUser(e);
                }
                else if(userToken.IsLoggedIn) {
                    switch (e.Path)
                    {
                        // Pattern Matching: Type Pattern and “when” keyword
                        case string s when s.StartsWith("/users/"):
                            if (e.Method == "GET")
                            {
                                user.GetUserData(e, userToken);
                            }
                            else if (e.Method == "PUT")
                            {
                                user.UpdateUserData(e, userToken);
                            }
                            break;
                        case "/sessions":
                            if (e.Method == "POST")
                            {
                                user.LoginUser(e);
                            }
                            break;
                        case "/packages":
                            if (e.Method == "POST")
                            {
                                if (userToken.IsAdmin) {
                                    Card card = new Card();
                                    card.CreateCards(e);
                                }
                                else
                                {
                                    e.Reply(400, "Only the admin can create packages."); //TODO
                                }
                            }
                            break;
                        case "/transactions/packages":
                            if (e.Method == "POST")
                            {
                                user.aquirePackage(e, userToken);
                            }
                            break;
                        case "/cards":
                            if (e.Method == "GET")
                            {
                                CardCollection cardCollections = new CardCollection();
                                cardCollections.GetCards(e, userToken);
                            }
                            break;
                        case string s when s.StartsWith("/deck"):
                            CardCollection cardCollection = new CardCollection();
                            if (e.Method == "GET")
                            {
                                cardCollection.PrintDeck(e, userToken);
                            }
                            else if (e.Method == "PUT")
                            {
                                cardCollection.UpdateDeck(e, userToken);
                            }
                            break;
                        case "/stats":
                            if (e.Method == "GET")
                            {
                                user.GetStats(e, userToken);
                            }
                            break;
                        case "/score":
                            if (e.Method == "GET")
                            {
                                user.GetScoreboard(e, userToken);
                            }
                            break;
                        case "/battles":
                            if (e.Method == "POST")
                            {
                                Lobby.Join(e, userToken);
                            }
                            break;
                        case "/tradings":
                            Trade newTrade = new Trade();
                            if (e.Method == "GET")
                            {
                                newTrade.GetTradingDeals(e, userToken);
                            }
                            else if (e.Method == "POST")
                            {
                                newTrade.PostTradingDeal(e, userToken);
                            }
                            break;
                        case string s when s.StartsWith("/tradings/"):
                            Trade tryTrade = new Trade();
                            if (e.Method == "POST")
                            {
                                tryTrade.TryTrade(e, userToken);
                            }
                            else if (e.Method == "DELETE")
                            {
                                tryTrade.DeleteTradingDeal(e, userToken);
                            }
                            break;
                        default:
                            Console.WriteLine("Rejected request.");
                            e.Reply(400);
                            break;
                    }
                }
                else
                {
                    e.Reply(400, "Missing or invalid token.");
                }
            }
            catch
            {
                e.Reply(400, "Request unsuccessful.");
            }
        }
    }
}
