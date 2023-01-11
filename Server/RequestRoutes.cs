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

namespace MTCG.Server
{
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////
    // constructors                                                                                             //
    //////////////////////////////////////////////////////////////////////////////////////////////////////////////
    public class RequestRoutes
    {
        public static void _Svr_Incoming(object evt)
        {
            HttpSvrEventArgs e = (HttpSvrEventArgs)evt;

            Console.WriteLine(e.PlainMessage + "\n\n");

            UserToken userToken = new UserToken(); 
            userToken.AuthenticateUser(e);

            if(e.Path == "/users" && e.Method== "POST") 
            {
                User.CreateUser(e);
            }
            else if (e.Path == "/sessions" && e.Method == "POST") 
            {
                User.LoginUser(e);
            }
            else if(userToken.IsLoggedIn) {
                switch (e.Path)
                {
                    // Pattern Matching: Type Pattern and “when” keyword
                    case string s when s.StartsWith("/users/"):
                        if (e.Method == "GET")
                        {
                            User.GetUserData(e, userToken);
                        }
                        else if (e.Method == "PUT")
                        {
                            User.UpdateUserData(e, userToken);
                        }
                        break;
                    case "/sessions":
                        if (e.Method == "POST")
                        {
                            User.LoginUser(e);
                        }
                        break;
                    case "/packages":
                        if (e.Method == "POST")
                        {
                            if (userToken.IsAdmin) {
                                Card.CreateCards(e);
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
                            User.aquirePackage(e, userToken);
                        }
                        break;
                    case "/cards":
                        if (e.Method == "GET")
                        {
                            Card.GetCards(e, userToken);
                        }
                        break;
                    case string s when s.StartsWith("/deck"):
                        if (e.Method == "GET")
                        {
                            CardCollection.GetDeck(e, userToken);
                        }
                        else if (e.Method == "PUT")
                        {
                            CardCollection.UpdateDeck(e, userToken);
                        }
                        break;
                    case "/stats":
                        if (e.Method == "GET")
                        {
                            Console.WriteLine("GET->stats.");
                        }
                        e.Reply(200);
                        break;
                    case "/score":
                        if (e.Method == "GET")
                        {
                            Console.WriteLine("GET->scoreboard.");
                        }
                        e.Reply(200);
                        break;
                    case "/battles":
                        if (e.Method == "POST")
                        {
                            Lobby.Join(e, userToken.LoggedInUser);
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
                        Console.WriteLine("Rejected message.");
                        e.Reply(400);
                        break;
                }
            }
            else
            {
                e.Reply(400, "Missing or invalid token.");
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // private methods                                                                                           //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

    }
}
