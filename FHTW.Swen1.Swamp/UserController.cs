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

                var getUserCommand = $"SELECT * FROM Users WHERE Username = '{username}'";
                var user = ExecuteQuery<User>(connection, getUserCommand).FirstOrDefault();

                connection.Close();

                return user;
            }
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

        public List<Card> GetAllAcquiredCards(string username)
        {
            using (var connection = new SqliteConnection(DataConnectionString))
            {
                connection.Open();

                var user = GetUserByUsername(username);

                if (user == null)
                {
                    Console.WriteLine($"User {username} not found. Returning empty card list");
                    return new List<Card>();
                }

                var getCardsCommand = $"SELECT * FROM Cards WHERE UserId = {user.Id}";
                return ExecuteQuery<Card>(connection, getCardsCommand);
            }
        }


        private List<T> ExecuteQuery<T>(SqliteConnection connection, string commandText) where T : new()
        {
            var result = new List<T>();

            using (var command = new SqliteCommand(commandText, connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var item = new T();

                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            var propertyName = reader.GetName(i);
                            var property = typeof(T).GetProperty(propertyName);

                            if (property != null)
                            {
                                var value = reader.GetValue(i);

                                // Prüfe auf DBNull.Value
                                if (value != DBNull.Value)
                                {
                                    // Konvertiere den Wert in den entsprechenden Typ
                                    if (property.PropertyType == typeof(int) && value.GetType() == typeof(long))
                                    {
                                        property.SetValue(item, Convert.ToInt32(value));
                                    }
                                    else if (property.PropertyType == typeof(Guid) && value.GetType() == typeof(string))
                                    {
                                        property.SetValue(item, Guid.Parse(value.ToString()));
                                    }
                                    else
                                    {
                                        property.SetValue(item, value);
                                    }
                                }
                            }
                        }

                        result.Add(item);
                    }
                }
            }

            return result;
        }



    }
}
