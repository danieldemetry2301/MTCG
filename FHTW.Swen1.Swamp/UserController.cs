using FHTW.Swen1.Swamp.Database;
using Microsoft.Data.Sqlite;
using System;

namespace FHTW.Swen1.Swamp
{
    public class UserController
    {
        private const string DataConnectionString = "Data Source=mctg.db";

        public string RegisterUser(User user)
        {
            using (var connection = new SqliteConnection(DataConnectionString))
            {
                connection.Open();

                if (UserExists(connection, user.Username))
                {
                    connection.Close();
                    return "409 User with the same username already registered";
                }

                DatabaseHelper.InsertUser(user);

                connection.Close();
                return "201 User successfully registered";
            }
        }


        public string LoginUser(string username, string password)
        {
            using (var connection = new SqliteConnection(DataConnectionString))
            {
                connection.Open();

                var user = GetUserByUsername(username);

                connection.Close();

                if (user != null && user.Password == password)
                {
                    return "200 User login successful";
                }
                else
                {
                    return "401 Invalid username/password provided";
                }
            }
        }

        private static void ExecuteCommand(SqliteConnection connection, string commandText)
        {
            using (var command = new SqliteCommand(commandText, connection))
            {
                command.ExecuteNonQuery();
            }
        }


        public User GetUserByUsername(string username)
        {
            using (var connection = new SqliteConnection(DataConnectionString))
            {
                connection.Open();
                var query = $"SELECT * FROM Users WHERE Username = '{username}'";

                using (var command = new SqliteCommand(query, connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new User(reader.GetString(0), reader.GetString(1)); // Hier den entsprechenden Index anpassen
                        }
                    }
                }

                connection.Close();
            }

            return null;
        }

        private static bool UserExists(SqliteConnection connection, string username)
        {
            var query = $"SELECT COUNT(*) FROM Users WHERE Username = '{username}'";

            using (var command = new SqliteCommand(query, connection))
            {
                var count = Convert.ToInt32(command.ExecuteScalar());
                return count > 0;
            }
        }
    }
}
