using MTCG_DEMETRY;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.IO;

namespace FHTW.Swen1.Swamp.Database
{
    public class DatabaseHelper
    {
        private const string DataConnectionString = "Host=localhost;Port=5432;Username=postgres;Password=admin;Database=mtcg";

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static void CreateDatabase()
        {
            using (var connection = new NpgsqlConnection(DataConnectionString))
            {
                connection.Open();
                connection.Close();
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public static void CreateTables()
        {
            using (var connection = new NpgsqlConnection(DataConnectionString))
            {
                connection.Open();

                ExecuteCommand(connection, @"
                    CREATE TABLE IF NOT EXISTS Users (
                        Id SERIAL PRIMARY KEY,
                        Username VARCHAR(255),
                        Password VARCHAR(255),
                        Coins INTEGER,
                        Name VARCHAR(255),
                        Bio TEXT,
                        Image TEXT,
                        Elo INTEGER,
                        Wins INTEGER DEFAULT 0,
                        Losses INTEGER DEFAULT 0
                    )");

                ExecuteCommand(connection, @"
                    CREATE TABLE IF NOT EXISTS Packages (
                        Id TEXT PRIMARY KEY
                    )");

                ExecuteCommand(connection, @"
                    CREATE TABLE IF NOT EXISTS Cards (
                        Id TEXT PRIMARY KEY,
                        Name TEXT NOT NULL,
                        Damage DOUBLE PRECISION NOT NULL,
                        Type TEXT,
                        PackageId TEXT,
                        UserId INTEGER,
                        FOREIGN KEY (UserId) REFERENCES Users(Id),
                        FOREIGN KEY (PackageId) REFERENCES Packages(Id)
                    )");

                ExecuteCommand(connection, @"
                    CREATE TABLE IF NOT EXISTS Decks (
                        UserId INTEGER NOT NULL,
                        CardId TEXT NOT NULL,
                        FOREIGN KEY (UserId) REFERENCES Users(Id),
                        FOREIGN KEY (CardId) REFERENCES Cards(Id),
                        PRIMARY KEY (UserId, CardId)
                    )");

                ExecuteCommand(connection, @"
                    CREATE TABLE IF NOT EXISTS TradingDeals (
                        Id TEXT PRIMARY KEY,
                        CardToTrade TEXT NOT NULL,
                        Type TEXT NOT NULL,
                        MinimumDamage DOUBLE PRECISION NOT NULL,
                        FOREIGN KEY (CardToTrade) REFERENCES Cards(Id)
                    )");

                connection.Close();
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public static void InsertTradingDeal(TradingDeal deal)
        {
            using (var connection = new NpgsqlConnection(DataConnectionString))
            {
                connection.Open();
                var command = new NpgsqlCommand("INSERT INTO TradingDeals (Id, CardToTrade, Type, MinimumDamage) VALUES (@id, @cardToTrade, @type, @minimumDamage)", connection);

                command.Parameters.AddWithValue("@id", deal.Id);
                command.Parameters.AddWithValue("@cardToTrade", deal.CardToTrade);
                command.Parameters.AddWithValue("@type", deal.Type);
                command.Parameters.AddWithValue("@minimumDamage", deal.MinimumDamage);

                command.ExecuteNonQuery();

                connection.Close();
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public static bool TradingDealExists(string dealId)
        {
            using (var connection = new NpgsqlConnection(DataConnectionString))
            {
                connection.Open();
                var command = new NpgsqlCommand("SELECT COUNT(*) FROM TradingDeals WHERE Id = @id", connection);

                command.Parameters.AddWithValue("@id", dealId);

                var count = Convert.ToInt32(command.ExecuteScalar());
                connection.Close();
                return count > 0;
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static bool GetUserDeck(long userId, string cardId)
        {
            using (var connection = new NpgsqlConnection(DataConnectionString))
            {
                connection.Open();
                var command = new NpgsqlCommand("SELECT COUNT(*) FROM Decks WHERE UserId = @userId AND CardId = @cardId", connection);
                command.Parameters.AddWithValue("@userId", userId);
                command.Parameters.AddWithValue("@cardId", cardId);

                var count = Convert.ToInt32(command.ExecuteScalar());
                connection.Close();
                return count > 0;

            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static void InsertUser(User user)
        {
            using (var connection = new NpgsqlConnection(DataConnectionString))
            {
                connection.Open();

                var insertUserCommand = new NpgsqlCommand(@"
                INSERT INTO Users (Username, Password, Coins, Name, Bio, Image, Elo, Wins, Losses) 
                VALUES (@username, @password, @coins, @name, @bio, @image, @elo, @wins, @losses)", connection);

                insertUserCommand.Parameters.AddWithValue("@username", user.Username);
                insertUserCommand.Parameters.AddWithValue("@password", user.Password);
                insertUserCommand.Parameters.AddWithValue("@coins", user.Coins);
                insertUserCommand.Parameters.AddWithValue("@name", user.Name ?? (object)DBNull.Value);
                insertUserCommand.Parameters.AddWithValue("@bio", user.Bio ?? (object)DBNull.Value);  
                insertUserCommand.Parameters.AddWithValue("@image", user.Image ?? (object)DBNull.Value); 
                insertUserCommand.Parameters.AddWithValue("@elo", user.Elo);
                insertUserCommand.Parameters.AddWithValue("@wins", user.Wins);
                insertUserCommand.Parameters.AddWithValue("@losses", user.Losses);

                insertUserCommand.ExecuteNonQuery();

                connection.Close();
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static void UpdateUserEloAndStats(User user)
        {
            using (var connection = new NpgsqlConnection(DataConnectionString))
            {
                connection.Open();

                var updateUserCommand = new NpgsqlCommand("UPDATE Users SET Elo = @elo, Wins = @wins, Losses = @losses WHERE Username = @username", connection);
                updateUserCommand.Parameters.AddWithValue("@elo", user.Elo);
                updateUserCommand.Parameters.AddWithValue("@wins", user.Wins);
                updateUserCommand.Parameters.AddWithValue("@losses", user.Losses);
                updateUserCommand.Parameters.AddWithValue("@username", user.Username);

                updateUserCommand.ExecuteNonQuery();

                connection.Close();
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public static bool PackageExistsWithCardId(List<Card> cards)
        {
            using (var connection = new NpgsqlConnection(DataConnectionString))
            {
                connection.Open();

                foreach (var card in cards)
                {
                    var sql = "SELECT COUNT(*) FROM Cards WHERE Id = @cardId AND PackageId IS NOT NULL";

                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@cardId", card.Id);

                        var result = (long)command.ExecuteScalar();

                        if (result == 0)
                        {
                            return false;
                        }
                    }
                }

                return true;
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public static void InsertPackage(Package package)
        {
            using (var connection = new NpgsqlConnection(DataConnectionString))
            {
                connection.Open();

                var packageCommand = new NpgsqlCommand("INSERT INTO Packages (Id) VALUES (@id)", connection);
                packageCommand.Parameters.AddWithValue("@id", package.Id);

                packageCommand.ExecuteNonQuery();

                connection.Close();
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public static void DeleteTradingDeal(string dealId)
        {
            using (var connection = new NpgsqlConnection(DataConnectionString))
            {
                connection.Open();

                var command = new NpgsqlCommand("DELETE FROM TradingDeals WHERE Id = @id", connection);
                command.Parameters.AddWithValue("@id", dealId);

                command.ExecuteNonQuery();

                connection.Close();
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static TradingDeal GetTradingDealById(string dealId)
        {
            using (var connection = new NpgsqlConnection(DataConnectionString))
            {
                connection.Open();

                var command = new NpgsqlCommand("SELECT Id, CardToTrade, Type, MinimumDamage FROM TradingDeals WHERE Id = @id", connection);
                command.Parameters.AddWithValue("@id", dealId);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new TradingDeal
                        {
                            Id = reader.GetString(0),
                            CardToTrade = reader.GetString(1),
                            Type = reader.GetString(2),
                            MinimumDamage = reader.GetDouble(3)
                        };
                    }
                }

                connection.Close();
            }

            return null;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public static List<TradingDeal> GetTradingDeals()
        {
            var tradingDeals = new List<TradingDeal>();
            using (var connection = new NpgsqlConnection(DataConnectionString))
            {
                connection.Open();
                var command = new NpgsqlCommand("SELECT Id, CardToTrade, Type, MinimumDamage FROM TradingDeals", connection);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var deal = new TradingDeal
                        {
                            Id = reader.GetString(0),
                            CardToTrade = reader.GetString(1),
                            Type = reader.GetString(2),
                            MinimumDamage = reader.GetDouble(3)
                        };
                        tradingDeals.Add(deal);
                    }
                }
                connection.Close();
            }
            return tradingDeals;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static void InsertCards(List<Card> cards)
        {
            using (var connection = new NpgsqlConnection(DataConnectionString))
            {
                connection.Open();

                foreach (var card in cards)
                {
                    var cardCommand = new NpgsqlCommand("INSERT INTO Cards (Id, Name, Damage, PackageId, Type) VALUES (@id, @name, @damage, @packageId, @type)", connection);
                    cardCommand.Parameters.AddWithValue("@id", card.Id);
                    cardCommand.Parameters.AddWithValue("@name", card.Name);
                    cardCommand.Parameters.AddWithValue("@damage", card.Damage);
                    cardCommand.Parameters.AddWithValue("@packageId", card.PackageId);
                    cardCommand.Parameters.AddWithValue("@type", card.Type);

                    cardCommand.ExecuteNonQuery();
                }

                connection.Close();
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static void AcquireCards(long userId, List<Card> cards)
        {
            using (var connection = new NpgsqlConnection(DataConnectionString))
            {
                connection.Open();

                foreach (var card in cards)
                {
                    var acquireCardCommand = new NpgsqlCommand("UPDATE Cards SET UserId = @userId WHERE Id = @cardId", connection);
                    acquireCardCommand.Parameters.AddWithValue("@userId", userId);
                    acquireCardCommand.Parameters.AddWithValue("@cardId", card.Id);

                    acquireCardCommand.ExecuteNonQuery();
                }

                connection.Close();
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public static bool UserOwnsCard(long userId, string cardId)
        {
            using (var connection = new NpgsqlConnection(DataConnectionString))
            {
                connection.Open();
                var command = new NpgsqlCommand("SELECT COUNT(*) FROM Cards WHERE Id = @cardId AND UserId = @userId", connection);
                command.Parameters.AddWithValue("@cardId", cardId);
                command.Parameters.AddWithValue("@userId", userId);

                var count = Convert.ToInt32(command.ExecuteScalar());
                connection.Close();
                return count > 0;
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static void UpdateUserCoins(long userId, long newCoins)
        {
            try
            {
                using (var connection = new NpgsqlConnection(DataConnectionString))
                {
                    connection.Open();
                    var updateUserCoinsCommand = new NpgsqlCommand("UPDATE Users SET Coins = @newCoins WHERE Id = @userId", connection);
                    updateUserCoinsCommand.Parameters.AddWithValue("@newCoins", newCoins);
                    updateUserCoinsCommand.Parameters.AddWithValue("@userId", userId);

                    int rowsAffected = updateUserCoinsCommand.ExecuteNonQuery();
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating user coins: {ex.Message}");
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        public static User GetOpponentForBattle(string requestingUsername)
        {
            using (var connection = new NpgsqlConnection(DataConnectionString))
            {
                connection.Open();

                var getOpponentCommand = $@"
                SELECT Id, Username, Password, Coins, Name, Bio, Image, Elo, Wins, Losses 
                FROM Users 
                WHERE Username != '{requestingUsername}' 
                ORDER BY RANDOM() 
                LIMIT 1";

                using (var command = new NpgsqlCommand(getOpponentCommand, connection))
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return new User
                        {
                            Id = reader.GetInt64(0),
                            Username = reader.GetString(1),
                            Password = reader.GetString(2),
                            Coins = reader.GetInt32(3),
                            Name = reader.GetString(4),
                            Bio = reader.IsDBNull(5) ? null : reader.GetString(5),
                            Image = reader.IsDBNull(6) ? null : reader.GetString(6),
                            Elo = reader.GetInt32(7),
                            Wins = reader.GetInt32(8),
                            Losses = reader.GetInt32(9)
                        };
                    }
                }

                connection.Close();
            }

            return null;
        }

        public static User GetUserByCardId(string cardId)
        {
            using (var connection = new NpgsqlConnection(DataConnectionString))
            {
                connection.Open();

                var command = new NpgsqlCommand("SELECT Users.* FROM Users INNER JOIN Cards ON Users.Id = Cards.UserId WHERE Cards.Id = @cardId", connection);
                command.Parameters.AddWithValue("@cardId", cardId);

                var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    return new User
                    {
                        Id = reader.GetInt64(0),
                        Username = reader.GetString(1),
                        Password = reader.GetString(2),
                        // ... Weitere User-Attribute auslesen
                    };
                }

                connection.Close();
            }
            return null;
        }

        public static Card GetCardById(string cardId)
        {
            using (var connection = new NpgsqlConnection(DataConnectionString))
            {
                connection.Open();

                var command = new NpgsqlCommand("SELECT * FROM Cards WHERE Id = @cardId", connection);
                command.Parameters.AddWithValue("@cardId", cardId);

                var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    return new Card
                    {
                        Id = reader.GetString(0),
                        Name = reader.GetString(1),
                        Damage = reader.GetDouble(2),
                        Type = reader.GetString(3)
                    };
                }

                connection.Close();
            }
            return null;
        }

        public static void ExchangeCards(long userId1, long userId2, string cardId1, string cardId2)
        {
            using (var connection = new NpgsqlConnection(DataConnectionString))
            {
                connection.Open();

                var command1 = new NpgsqlCommand("UPDATE Cards SET UserId = @userId WHERE Id = @cardId", connection);
                command1.Parameters.AddWithValue("@userId", userId2);
                command1.Parameters.AddWithValue("@cardId", cardId1);
                command1.ExecuteNonQuery();

                var command2 = new NpgsqlCommand("UPDATE Cards SET UserId = @userId WHERE Id = @cardId", connection);
                command2.Parameters.AddWithValue("@userId", userId1);
                command2.Parameters.AddWithValue("@cardId", cardId2);
                command2.ExecuteNonQuery();

                connection.Close();
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
        public static void ResetDatabase()
        {
            using (var connection = new NpgsqlConnection(DataConnectionString))
            {
                connection.Open();

                ExecuteCommand(connection, "DROP TABLE IF EXISTS Cards CASCADE");
                ExecuteCommand(connection, "DROP TABLE IF EXISTS Packages CASCADE");
                ExecuteCommand(connection, "DROP TABLE IF EXISTS Users CASCADE");
                ExecuteCommand(connection, "DROP TABLE IF EXISTS Decks CASCADE");
                ExecuteCommand(connection, "DROP TABLE IF EXISTS TradingDeals CASCADE");
                CreateTables();

                connection.Close();
            }
        }
    }
}
