using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;
using System.Xml.Linq;
using Npgsql;
using System.Text.Json.Nodes;
using System.Text.Json;
using System.Net.NetworkInformation;
using System.Text.Json.Serialization;
using Microsoft.VisualBasic;
using MTCG.Controller;
using System.Drawing;

namespace MTCG.Models
{
    internal class User
    {
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public properties                                                                                         //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public string? Username { get; set; }

        public string? Password { get; set; }

        public string? Name { get; set; }

        public string? Bio { get; set; }

        public string? Image { get; set; }

        //////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // public methods                                                                                           //
        //////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public void CreateUser(HttpSvrEventArgs e)
        {
            try
            {
                User? newuser = JsonSerializer.Deserialize<User>(e.Payload);
                if(newuser.Username == "" || newuser.Password == "")
                {
                    e.Reply(400, "Error occured while creating user.");
                    return;
                }
                var connectionString = "Host=localhost;Username=swe1user;Password=swe1pw;Database=swe1db";
                using var dataSource = NpgsqlDataSource.Create(connectionString);

                 using (var cmd = dataSource.CreateCommand("INSERT INTO users (username, password, coins) VALUES ((@p1), (@p2), (@p3))"))
                {
                    cmd.Parameters.AddWithValue("@p1", newuser.Username);
                    cmd.Parameters.AddWithValue("@p2", BCrypt.Net.BCrypt.HashPassword(newuser.Password));
                    cmd.Parameters.AddWithValue("@p3", 20);
                    cmd.ExecuteNonQuery();
                }

                using (var cmd = dataSource.CreateCommand("INSERT INTO stats (wins, losses, elo, username) VALUES ((@p1), (@p2), (@p3), (@p4))"))
                {
                    cmd.Parameters.AddWithValue("@p1", 0);
                    cmd.Parameters.AddWithValue("@p2", 0);
                    cmd.Parameters.AddWithValue("@p3", 100);
                    cmd.Parameters.AddWithValue("@p4", newuser.Username);
                    cmd.ExecuteNonQuery();
                }
                e.Reply(200, "User created successfully");
            }
            catch (NpgsqlException ex){
                if (ex.SqlState == "23505")
                {
                    e.Reply(400, "Error: Username already in use.");
                }
                else
                {
                    e.Reply(400, "Error occured while creating user: " + ex.Message);
                }
            }
            catch (Exception ex)
            {
                e.Reply(400, "Error occured while creating user: " + ex.Message);
            }
        }

        public void LoginUser(HttpSvrEventArgs e)
        {
            try
            {
                User? user = JsonSerializer.Deserialize<User>(e.Payload);
                string? passwordHash = null;
                var connectionString = "Host=localhost;Username=swe1user;Password=swe1pw;Database=swe1db";
                using var dataSource = NpgsqlDataSource.Create(connectionString);

                // Retrieve all rows
                using (var cmd = dataSource.CreateCommand("SELECT password FROM users WHERE username = (@p1)"))
                {
                    cmd.Parameters.AddWithValue("@p1", user.Username);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            passwordHash = reader.GetString(0);
                        }
                    }
                }
                if (BCrypt.Net.BCrypt.Verify(user.Password, passwordHash))
                {
                    e.Reply(200, "User logged in successfully");
                }
                else
                {
                    e.Reply(400, "Couldn't log in.");
                }
            }
            catch (Exception ex)
            {
                e.Reply(400, "Error occured while logging in: " + ex.Message);
            }
        }

        public void aquirePackage(HttpSvrEventArgs e, UserToken userToken)
        {
            try
            {
                var connectionString = "Host=localhost;Username=swe1user;Password=swe1pw;Database=swe1db";
                using var dataSource = NpgsqlDataSource.Create(connectionString);
                Guid[] cards = new Guid[5];
                int pid = 0;
                int coins = 0;
                // Check if user has enough coins
                using (var cmd = dataSource.CreateCommand("SELECT coins FROM users WHERE username = (@p1)"))
                {
                    cmd.Parameters.AddWithValue("@p1", userToken.LoggedInUser);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            coins = reader.GetInt32(0);
                        }
                    }
                }
                if(coins < 5)
                {
                    e.Reply(400, "Not enough coins to buy a package.");
                    return;
                }

                // Retrieve top package from db
                using (var cmd = dataSource.CreateCommand("SELECT pid, card1, card2, card3, card4, card5 FROM packages ORDER BY pid ASC LIMIT 1"))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        pid = reader.GetInt32(0);
                        cards[0] = reader.GetGuid(1);
                        cards[1] = reader.GetGuid(2);
                        cards[2] = reader.GetGuid(3);
                        cards[3] = reader.GetGuid(4);
                        cards[4] = reader.GetGuid(5);
                    }
                }

                if (cards[0] == Guid.Empty)
                {
                    e.Reply(400, "No more packages available.");
                    return;
                }

                foreach (Guid cardid in cards) { 
                    using (var cmd = dataSource.CreateCommand("UPDATE cards SET username = (@p1) WHERE id = (@p2)"))
                    {
                        cmd.Parameters.AddWithValue("@p1", userToken.LoggedInUser);
                        cmd.Parameters.AddWithValue("@p2", cardid);
                        cmd.ExecuteNonQuery();
                    }
                }
                using (var cmd = dataSource.CreateCommand("DELETE FROM packages WHERE pid = (@p1)"))
                {
                    cmd.Parameters.AddWithValue("@p1", pid);
                    cmd.ExecuteNonQuery();
                }

                using (var cmd = dataSource.CreateCommand("UPDATE users SET coins = (@p1) WHERE username = (@p2)"))
                {
                    cmd.Parameters.AddWithValue("@p1", coins-5);
                    cmd.Parameters.AddWithValue("@p2", userToken.LoggedInUser);
                    cmd.ExecuteNonQuery();
                }

                e.Reply(200, "Package acquired successfully.");
            }
            catch (Exception ex)
            {
                e.Reply(400, "Error occured while acquiring package: " + ex.Message);
            }
        }

        public void GetUserData(HttpSvrEventArgs e, UserToken userToken)
        {
            try
            {
                string[] pathUser = e.Path.Split("/");
                //check if username matches token
                if (userToken.LoggedInUser == pathUser[2])
                {
                    var connectionString = "Host=localhost;Username=swe1user;Password=swe1pw;Database=swe1db";
                    using var dataSource = NpgsqlDataSource.Create(connectionString);

                    // Retrieve all cards belonging to the user
                    string replyString = "Your Profile: \n";
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
                else
                {
                    e.Reply(400, "Authorization doesn't match request.");
                }
            }
            catch (Exception ex)
            {
                e.Reply(400, "Error occured while fetching profile data: " + ex.Message);
            }
        }

        public void UpdateUserData(HttpSvrEventArgs e, UserToken userToken)
        {
            try
            {
                string[] pathUser = e.Path.Split("/");
                //check if username matches token
                if (userToken.LoggedInUser == pathUser[2])
                {
                    User? userUpdate = JsonSerializer.Deserialize<User>(e.Payload);

                    var connectionString = "Host=localhost;Username=swe1user;Password=swe1pw;Database=swe1db";
                    using var dataSource = NpgsqlDataSource.Create(connectionString);

                    // Retrieve all cards belonging to the user
                    string replyString = "Your Cards: \n";
                    using (var cmd = dataSource.CreateCommand("UPDATE users SET name = (@p1), bio = (@p2), image = (@p3) WHERE username = (@p4)"))
                    {
                        cmd.Parameters.AddWithValue("@p1", userUpdate.Name);
                        cmd.Parameters.AddWithValue("@p2", userUpdate.Bio);
                        cmd.Parameters.AddWithValue("@p3", userUpdate.Image);
                        cmd.Parameters.AddWithValue("@p4", userToken.LoggedInUser);
                        cmd.ExecuteNonQuery();
                    }
                    
                    e.Reply(200, "Profile update successful.");
     
                }
                else
                {
                    e.Reply(400, "Authorization doesn't match request.");
                }
            }
            catch (Exception ex)
            {
                e.Reply(400, "Error occured while updating profile: " + ex.Message);
            }
        }

        //TODO: Trade request

        public void GetStats(HttpSvrEventArgs e, UserToken userToken)
        {
            try
            {
                var connectionString = "Host=localhost;Username=swe1user;Password=swe1pw;Database=swe1db";
                using var dataSource = NpgsqlDataSource.Create(connectionString);

                // Retrieve user stats
                string replyString = "Your Stats: \n";
                using (var cmd = dataSource.CreateCommand("SELECT wins, losses, elo FROM stats WHERE username = (@p1)"))
                {
                    cmd.Parameters.AddWithValue("@p1", userToken.LoggedInUser);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            replyString += "username: " + userToken.LoggedInUser +
                                "\nwins: " + reader.GetInt64(0) +
                                "\nlosses: " + reader.GetInt64(1) +
                                "\nelo: " + reader.GetInt64(2) + "\n";
                        }
                    }
                }
                e.Reply(200, replyString);
            }
            catch (Exception ex)
            {
                e.Reply(400, "Error occured while fetching profile data: " + ex.Message);
            }
        }

        public void GetScoreboard(HttpSvrEventArgs e, UserToken userToken)
        {
            try
            {
                var connectionString = "Host=localhost;Username=swe1user;Password=swe1pw;Database=swe1db";
                using var dataSource = NpgsqlDataSource.Create(connectionString);

                //retrieve top 10 scoreboard
                string replyString = "Top 10 Scores: \n";
                using (var cmd = dataSource.CreateCommand("SELECT username, wins, losses, elo FROM stats ORDER BY elo DESC, wins DESC, losses ASC, username DESC LIMIT 10;"))
                {
                    //TODO: maybe show user under top 10 ? cmd.Parameters.AddWithValue("@p1", userToken.LoggedInUser);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            replyString += "username: " + reader.GetString(0) +
                                "\nwins: " + reader.GetInt64(1) +
                                "\nlosses: " + reader.GetInt64(2) +
                                "\nelo: " + reader.GetInt64(3) + "\n";
                        }
                    }
                }
                e.Reply(200, replyString);
            }
            catch (Exception ex)
            {
                e.Reply(400, "Error occured while fetching profile data: " + ex.Message);
            }
        }
    }
}
