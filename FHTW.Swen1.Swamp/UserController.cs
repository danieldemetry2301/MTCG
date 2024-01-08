using FHTW.Swen1.Swamp.Database;
using Npgsql;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    namespace FHTW.Swen1.Swamp
    {
    public class UserController
    {
        private const string DataConnectionString = "Host=localhost;Port=5432;Username=postgres;Password=admin;Database=mtcg";
        private static Dictionary<string, User> userCache = new Dictionary<string, User>();


        public string RegisterUser(User user)
        {
            using (var connection = new NpgsqlConnection(DataConnectionString))
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
            using (var connection = new NpgsqlConnection(DataConnectionString))
            {
                connection.Open();

                var user = GetUserByUsername(username);

                connection.Close();

                if (user != null)
                {
                    if (!string.IsNullOrEmpty(user.Password) && user.Password == password)
                    {
                        return "200 User login successful";
                    }
                    else
                    {
                        return "401 Invalid username/password provided";
                    }
                }
                else
                {
                    return "401 Invalid username/password provided";
                }
            }
        }



        public User GetUserByUsername(string username)
        {
            if (userCache.TryGetValue(username, out User cachedUser))
            {
                return cachedUser;
            }

            using (var connection = new NpgsqlConnection(DataConnectionString))
            {
                connection.Open();

                var getUserCommand = $"SELECT * FROM Users WHERE Username = '{username}'";
                var reader = new NpgsqlCommand(getUserCommand, connection).ExecuteReader();

                User user = null;

                if (reader.Read())
                {
                    user = new User
                    {
                        Id = reader.GetInt64(0),
                        Username = reader.GetString(1),
                        Password = reader.GetString(2),
                        Coins = reader.IsDBNull(3) ? 0 : reader.GetInt32(3) // Überprüfen auf NULL und Standardwert zuweisen
                    };

                    userCache[username] = user; // Benutzer im Cache speichern
                }

                connection.Close();

                return user;
            }
        }



        public List<Card> GetAllAcquiredCards(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                Console.WriteLine("Username is null or empty. Returning empty card list.");
                return new List<Card>();
            }

            using (var connection = new NpgsqlConnection(DataConnectionString))
            {
                connection.Open();

                var user = GetUserByUsername(username);

                if (user == null)
                {
                    Console.WriteLine($"User {username} not found. Returning empty card list.");
                    return new List<Card>();
                }

                var getCardsCommand = $"SELECT * FROM Cards WHERE UserId = {user.Id}";
                return ExecuteQuery<Card>(connection, getCardsCommand);
            }
        }


        private static bool UserExists(NpgsqlConnection connection, string username)
        {
            var query = $"SELECT COUNT(*) FROM Users WHERE Username = '{username}'";

            using (var command = new NpgsqlCommand(query, connection))
            {
                var count = Convert.ToInt32(command.ExecuteScalar());
                return count > 0;
            }
        }

        private static void ExecuteCommand(NpgsqlConnection connection, string commandText)
        {
            using (var command = new NpgsqlCommand(commandText, connection))
            {
                command.ExecuteNonQuery();
            }
        }

        private List<T> ExecuteQuery<T>(NpgsqlConnection connection, string commandText) where T : new()
        {
            var result = new List<T>();
            var properties = typeof(T).GetProperties().ToDictionary(p => p.Name.ToLower(), p => p);

            using (var command = new NpgsqlCommand(commandText, connection))
            {
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var item = new T();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            var columnName = reader.GetName(i).ToLower();
                            if (properties.TryGetValue(columnName, out var property) && !reader.IsDBNull(i))
                            {
                                var value = reader.GetValue(i);
                                if (property.PropertyType == typeof(string) && value is Guid)
                                {
                                    value = value.ToString();
                                }
                                else if (property.PropertyType != typeof(String) && property.PropertyType.GetInterface(nameof(IConvertible)) != null)
                                {
                                    value = Convert.ChangeType(value, property.PropertyType);
                                }
                                property.SetValue(item, value);
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
