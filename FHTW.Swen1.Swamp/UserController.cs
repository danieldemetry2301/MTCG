using FHTW.Swen1.Swamp.Database;
using MTCG_DEMETRY;
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

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
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

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

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

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////


        public User GetUserByUsername(string username)
        {
            using (var connection = new NpgsqlConnection(DataConnectionString))
            {
                connection.Open();

                var getUserCommand = new NpgsqlCommand("SELECT * FROM Users WHERE Username = @username", connection);
                getUserCommand.Parameters.AddWithValue("@username", username);

                var reader = getUserCommand.ExecuteReader();

                User user = null;

                if (reader.Read())
                {
                    user = new User
                    {
                        Id = reader.GetInt64(0),
                        Username = reader.GetString(1),
                        Password = reader.GetString(2),
                        Coins = reader.IsDBNull(3) ? 0 : reader.GetInt32(3)
                    };
                }

                connection.Close();

                return user;
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public List<UserStats> GetUserScoreboard()
        {
            var scoreboard = new List<UserStats>();
            using (var connection = new NpgsqlConnection(DataConnectionString))
            {
                connection.Open();

                var getScoreboardCommand = "SELECT Name, Elo, Wins, Losses FROM Users WHERE Name IS NOT NULL AND Name <> '' ORDER BY Elo DESC";
                using (var command = new NpgsqlCommand(getScoreboardCommand, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var name = reader.IsDBNull(0) ? string.Empty : reader.GetString(0);
                        var elo = reader.GetInt32(1);
                        var wins = reader.GetInt32(2);
                        var losses = reader.GetInt32(3);

                        scoreboard.Add(new UserStats
                        {
                            Name = name,
                            Elo = elo,
                            Wins = wins,
                            Losses = losses
                        });
                    }
                }

                connection.Close();
            }

            return scoreboard;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public User GetUserProfile(string username)
        {
            using (var connection = new NpgsqlConnection(DataConnectionString))
            {
                connection.Open();

                var getUserCommand = new NpgsqlCommand("SELECT Name, Bio, Image FROM Users WHERE Username = @username", connection);
                getUserCommand.Parameters.AddWithValue("@username", username);

                var reader = getUserCommand.ExecuteReader();

                User user = null;

                if (reader.Read())
                {
                    user = new User
                    {
                        Name = reader.IsDBNull(0) ? null : reader.GetString(0),
                        Bio = reader.IsDBNull(1) ? null : reader.GetString(1),
                        Image = reader.IsDBNull(2) ? null : reader.GetString(2)
                    };
                }

                connection.Close();

                return user;
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public UserStats GetUserStats(string username)
        {
            using (var connection = new NpgsqlConnection(DataConnectionString))
            {
                connection.Open();

                var getUserStatsCommand = new NpgsqlCommand("SELECT Name, Elo, Wins, Losses FROM Users WHERE Username = @username", connection);
                getUserStatsCommand.Parameters.AddWithValue("@username", username);

                var reader = getUserStatsCommand.ExecuteReader();

                if (reader.Read())
                {
                    return new UserStats
                    {
                        Name = reader.GetString(0),
                        Elo = reader.GetInt32(1),
                        Wins = reader.GetInt32(2),
                        Losses = reader.GetInt32(3)
                    };
                }

                connection.Close();
            }

            return null;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public void UpdateUserProfile(string username, User updatedUser)
        {
            using (var connection = new NpgsqlConnection(DataConnectionString))
            {
                connection.Open();

                var updateUserCommand = new NpgsqlCommand(@"
            UPDATE Users 
            SET Name = @name, Bio = @bio, Image = @image 
            WHERE Username = @username", connection);

                updateUserCommand.Parameters.AddWithValue("@name", updatedUser.Name);
                updateUserCommand.Parameters.AddWithValue("@bio", updatedUser.Bio);
                updateUserCommand.Parameters.AddWithValue("@image", updatedUser.Image);
                updateUserCommand.Parameters.AddWithValue("@username", username);

                updateUserCommand.ExecuteNonQuery();

                connection.Close();
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public List<Card> GetUserDeck(string username)
        {
            using (var connection = new NpgsqlConnection(DataConnectionString))
            {
                connection.Open();

                var user = GetUserByUsername(username);
                if (user == null) return new List<Card>();

                var getDeckCommand = new NpgsqlCommand(@"SELECT Cards.* FROM Cards INNER JOIN Decks ON Cards.Id = Decks.CardId WHERE Decks.UserId = @userId", connection);
                getDeckCommand.Parameters.AddWithValue("@userId", user.Id);

                return ExecuteQuery<Card>(connection, getDeckCommand);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public List<Card> GetUserAcquiredCards(string username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return new List<Card>();
            }

            using (var connection = new NpgsqlConnection(DataConnectionString))
            {
                connection.Open();

                var user = GetUserByUsername(username);

                if (user == null)
                {
                    return new List<Card>();
                }

                NpgsqlCommand getCardsCommand = new NpgsqlCommand("SELECT * FROM Cards WHERE UserId = @userId", connection);
                getCardsCommand.Parameters.AddWithValue("@userId", user.Id);

                return ExecuteQuery<Card>(connection, getCardsCommand);
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private static bool UserExists(NpgsqlConnection connection, string username)
        {
            var query = "SELECT COUNT(*) FROM Users WHERE Username = @username";

            using (var command = new NpgsqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@username", username);

                var count = Convert.ToInt32(command.ExecuteScalar());
                return count > 0;
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public string ConfigureUserDeck(string username, List<string> cardIds)
        {
            if (cardIds.Count < 4)
            {
                return "400 The provided deck did not include the required amount of cards";
            }

            using (var connection = new NpgsqlConnection(DataConnectionString))
            {
                connection.Open();

                var user = GetUserByUsername(username);
                if (user == null)
                {
                    return "401 Access token is missing or invalid";
                }

                var cardsBelongToUser = cardIds.All(cardId => UserOwnsCard(connection, user.Id, cardId));
                if (!cardsBelongToUser)
                {
                    return "403 At least one of the provided cards does not belong to the user or is not available";
                }

                foreach (var cardId in cardIds)
                {
                    var insertDeckCommand = new NpgsqlCommand("INSERT INTO Decks (UserId, CardId) VALUES (@userId, @cardId)", connection);
                    insertDeckCommand.Parameters.AddWithValue("@userId", user.Id);
                    insertDeckCommand.Parameters.AddWithValue("@cardId", cardId);
                    insertDeckCommand.ExecuteNonQuery();
                }

                connection.Close();
            }

            return "200 The deck has been successfully configured";
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private bool UserOwnsCard(NpgsqlConnection connection, long userId, string cardId)
        {
            var checkCardCommand = "SELECT COUNT(*) FROM Cards WHERE Id = @cardId AND UserId = @userId";
            using (var command = new NpgsqlCommand(checkCardCommand, connection))
            {
                command.Parameters.AddWithValue("@cardId", cardId);
                command.Parameters.AddWithValue("@userId", userId);

                var count = Convert.ToInt32(command.ExecuteScalar());
                return count > 0;
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private static void ExecuteCommand(NpgsqlConnection connection, string commandText)
        {
            using (var command = new NpgsqlCommand(commandText, connection))
            {
                command.ExecuteNonQuery();
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        private List<T> ExecuteQuery<T>(NpgsqlConnection connection, NpgsqlCommand command) where T : new()
        {
            var result = new List<T>();
            var properties = typeof(T).GetProperties().ToDictionary(p => p.Name.ToLower(), p => p);

            command.Connection = connection;

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

            return result;
        }
    }
}
