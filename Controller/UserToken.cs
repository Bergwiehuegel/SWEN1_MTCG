using MTCG.Models;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MTCG.Controller
{
    public class UserToken
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public properties                                                                                         //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public Boolean IsLoggedIn { get; set; } = false;

        public Boolean IsAdmin { get; set; } = false;

        public string? LoggedInUser { get; set; }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public methods                                                                                            //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////


        // checks if the name in the token (request) matches the name in the path and sets the token (class) and IsLoggedIn if it matches
        // also checks if the user is the admin and sets IsAdmin
        public void AuthenticateUser(HttpSvrEventArgs e)
        {
            var index = Array.FindIndex(e.Headers, x => x.Name.Contains("Authorization"));
            if (index >= 0)
            {
                string[] authHeader = e.Headers[index].Value.Split(' ');
                string[] TokenName = authHeader[1].Split('-');
                string passedUsername = TokenName[0];
                int checkUsername = 0;
                try
                {
                    var connectionString = "Host=localhost;Username=swe1user;Password=swe1pw;Database=swe1db";
                    using var dataSource = NpgsqlDataSource.Create(connectionString);
                    
                    // Check if user exists
                    using (var cmd = dataSource.CreateCommand("SELECT 1 FROM users WHERE username = (@p1)"))
                    {
                        cmd.Parameters.AddWithValue("@p1", passedUsername);
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                checkUsername = reader.GetInt16(0);
                            }
                        }
                    }
                    if(checkUsername == 1)
                    {
                        IsLoggedIn = true;
                        if(passedUsername == "admin")
                        {
                            IsAdmin = true;
                        }
                        LoggedInUser = passedUsername;
                    }
                }
                catch (Exception ex)
                {
                    e.Reply(400, "Error occured while logging in: " + ex.Message);
                }

            }
        }
    }
}
