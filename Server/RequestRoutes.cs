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

            UserToken.AuthenticateUser(e);

            if(e.Path == "/users" && e.Method== "POST") 
            {
                User.CreateUser(e);
            }
            else if (e.Path == "/sessions" && e.Method == "POST") 
            {
                User.LoginUser(e);
            }
            else if(UserToken.IsLoggedIn) {
                switch (e.Path)
                {
                    // Pattern Matching: Type Pattern and “when” keyword
                    case string s when s.StartsWith("/users/"):
                        if (e.Method == "GET")
                        {
                            Card.GetCards(e);
                        }
                        else if (e.Method == "PUT")
                        {
                            Console.WriteLine("PUT->users->username");
                        e.Reply(200);
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
                            if (UserToken.IsAdmin) {
                                Card.CreateCards(e);
                            }
                            else
                            {
                                e.Reply(400); //TODO
                            }
                        }
                        break;
                    case "/transactions/packages":
                        if (e.Method == "POST")
                        {
                            User.aquirePackage(e);
                        }
                        break;
                    case "/cards":
                        if (e.Method == "GET")
                        {
                            Console.WriteLine("GET->cards.");
                            Console.WriteLine(UserToken.IsLoggedIn);
                            Console.WriteLine(UserToken.IsAdmin);
                            Console.WriteLine(UserToken.LoggedInUser);
                        }
                        e.Reply(200);
                        break;
                    case string s when s.StartsWith("/deck"):
                        if (e.Method == "GET")
                        {
                            Console.WriteLine("GET->deck.");
                        }
                        else if (e.Method == "PUT")
                        {
                            Console.WriteLine("PUT->deck.");
                        }
                        e.Reply(200);
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
                            Console.WriteLine("POST->lobby.");
                        }
                        e.Reply(200);
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
