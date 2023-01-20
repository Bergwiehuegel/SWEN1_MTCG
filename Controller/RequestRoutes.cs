﻿using MTCG.Models;
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
    // utility class with just a function that routes based on path in the request
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // public methods                                                                                            //
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////
    public class RequestRoutes
    {
        public static void _Svr_Incoming(object evt)
        {
            HttpSvrEventArgs e = (HttpSvrEventArgs)evt;

            Console.WriteLine(e.PlainMessage + "\n\n");

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
                            Card card = new Card();
                            card.GetCards(e, userToken);
                        }
                        break;
                    case string s when s.StartsWith("/deck"):
                        CardCollection cardCollection = new CardCollection();
                        if (e.Method == "GET")
                        {
                            cardCollection.GetDeck(e, userToken);
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
                        if (e.Method == "GET")
                        {
                            Console.WriteLine("GET->tr.");
                        }
                        else if (e.Method == "POST")
                        {
                            Console.WriteLine("POST->tr.");
                        }
                        else if (e.Method == "DELETE")
                        {
                            Console.WriteLine("DELETE->tr.");
                        }
                        e.Reply(200);
                        break;
                    case string s when s.StartsWith("/tradings/"):
                        if (e.Method == "POST")
                        {
                            Console.WriteLine("POST->tr->id.");
                        }
                        e.Reply(200);
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
    }
}
