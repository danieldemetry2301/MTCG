    using Npgsql;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    namespace FHTW.Swen1.Swamp
    {
        public class UserController
        {
            private const string DataConnectionString = "Host=localhost;Port=5432;Username=postgres;Password=admin;Database=mtcg";


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

                    InsertUserToPostgres(user);

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
            using (var connection = new NpgsqlConnection(DataConnectionString))
            {
                connection.Open();

                var getUserCommand = $"SELECT Id, Username, Password FROM Users WHERE Username = '{username}'";
                var reader = new NpgsqlCommand(getUserCommand, connection).ExecuteReader();

                User user = null;

                while (reader.Read())
                {
                    user = new User
                    {
                        Id = reader.GetInt64(0),
                        Username = reader.GetString(1),
                        Password = reader.GetString(2)
                    };
                }

                connection.Close();

                return user;
            }
        }


        public List<Card> GetAllAcquiredCards(string username)
        {
            using (var connection = new NpgsqlConnection(DataConnectionString))
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

        private static void InsertUserToPostgres(User user)
            {
                using (var connection = new NpgsqlConnection(DataConnectionString))
                {
                    connection.Open();

                    var insertUserCommand = $"INSERT INTO Users (Username, Password) VALUES ('{user.Username}', '{user.Password}')";
                    ExecuteCommand(connection, insertUserCommand);

                    connection.Close();
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

                using (var command = new NpgsqlCommand(commandText, connection))
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
