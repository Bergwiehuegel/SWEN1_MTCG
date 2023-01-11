using MTCG.Models;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MTCG.Server
{
    public class UserToken
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public properties (private setters)                                                                       //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public Boolean IsLoggedIn { get; private set; } = false;

        public Boolean IsAdmin { get; private set; } = false;

        public string? LoggedInUser { get; private set; }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public methods                                                                                            //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

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
